using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Wedding_Proposal_BE.Features.Profiles.Application.DTOs;
using Wedding_Proposal_BE.Features.Profiles.Application.Interfaces;
using Wedding_Proposal_BE.Features.Profiles.Domain.Entities;
using Wedding_Proposal_BE.Features.Profiles.Domain.Enums;
using Wedding_Proposal_BE.Features.Profiles.Domain.ValueObjects;
using Wedding_Proposal_BE.Shared.Common;
using Wedding_Proposal_BE.Shared.Infrastructure.Data;
using Wedding_Proposal_BE.Shared.Infrastructure.Identity;
using Wedding_Proposal_BE.Shared.Infrastructure.Storage;
using Profile = Wedding_Proposal_BE.Features.Profiles.Domain.Entities.Profile;

namespace Wedding_Proposal_BE.Features.Profiles.Infrastructure.Services;

public class ProfileService : IProfileService
{
    private static readonly string[] AllowedPhotoContentTypes = ["image/jpeg", "image/png", "image/webp"];
    private const long MaxPhotoBytes = 5 * 1024 * 1024;

    private readonly AppDbContext _dbContext;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IObjectStorageService _objectStorage;

    public ProfileService(AppDbContext dbContext, UserManager<ApplicationUser> userManager, IObjectStorageService objectStorage)
    {
        _dbContext = dbContext;
        _userManager = userManager;
        _objectStorage = objectStorage;
    }

    public async Task<Result<ProfileResponse>> GetByUserIdAsync(Guid userId)
    {
        var profile = await LoadProfileAsync(p => p.UserId == userId);
        if (profile is null)
        {
            // Mirrors the old FE mock's "creates a blank one on first access" behavior — the
            // account's own FirstName/LastName seed the blank profile's display name, and a
            // placeholder DateOfBirth (corrected later via UpdateAsync) unblocks profile
            // creation without a separate onboarding round trip.
            var owner = await _userManager.FindByIdAsync(userId.ToString());
            if (owner is null)
            {
                return Result.Failure<ProfileResponse>("Account not found.", "USER_NOT_FOUND");
            }

            var placeholderDateOfBirth = DateOnly.FromDateTime(DateTime.UtcNow.AddYears(-18));
            profile = Profile.CreateBlank(userId, owner.FirstName, owner.LastName, placeholderDateOfBirth);
            _dbContext.Profiles.Add(profile);
            await _dbContext.SaveChangesAsync();
        }

        return Result.Success(await ToResponseAsync(profile, userId));
    }

    public async Task<Result<ProfileResponse>> GetByIdAsync(Guid profileId, Guid? viewerUserId)
    {
        var profile = await LoadProfileAsync(p => p.Id == profileId);
        if (profile is null)
        {
            return Result.Failure<ProfileResponse>("This profile could not be found.", "PROFILE_NOT_FOUND");
        }

        return Result.Success(await ToResponseAsync(profile, viewerUserId));
    }

    public async Task<Result<ProfileResponse>> CreateAsync(Guid userId, CreateProfileRequest request)
    {
        var existing = await _dbContext.Profiles.FirstOrDefaultAsync(p => p.UserId == userId);
        if (existing is not null)
        {
            return Result.Failure<ProfileResponse>("Profile already exists for this user.", "PROFILE_EXISTS");
        }

        var profile = Profile.CreateBlank(userId, request.FirstName, request.LastName, request.DateOfBirth);
        _dbContext.Profiles.Add(profile);
        await _dbContext.SaveChangesAsync();

        return Result.Success(await ToResponseAsync(profile, userId));
    }

    public async Task<Result<ProfileResponse>> UpdateAsync(Guid userId, UpdateProfileRequest request)
    {
        var profile = await LoadProfileAsync(p => p.UserId == userId);
        if (profile is null)
        {
            return Result.Failure<ProfileResponse>("Profile not found.", "PROFILE_NOT_FOUND");
        }

        if (request.DateOfBirth is { } dateOfBirth)
        {
            profile.UpdateDateOfBirth(dateOfBirth);
        }

        profile.UpdateBasics(
            request.FirstName ?? profile.FirstName,
            request.LastName ?? profile.LastName,
            request.ManagedBy ?? profile.ManagedBy,
            request.AboutMe ?? profile.AboutMe,
            request.Hobbies ?? profile.Hobbies,
            request.Interests ?? profile.Interests);

        if (request.BasicInfo is { } basicInfo)
        {
            profile.UpdateBasicInfo(BasicInfo.Create(
                basicInfo.Gender ?? profile.BasicInfo.Gender,
                basicInfo.Race ?? profile.BasicInfo.Race,
                basicInfo.Religion ?? profile.BasicInfo.Religion,
                basicInfo.Caste ?? profile.BasicInfo.Caste,
                basicInfo.MaritalStatus ?? profile.BasicInfo.MaritalStatus,
                basicInfo.HeightLabel ?? profile.BasicInfo.HeightLabel));
        }

        if (request.Education is { } education)
        {
            profile.UpdateEducation(Education.Create(
                education.Qualification ?? profile.Education.Qualification,
                education.QualificationStatus ?? profile.Education.QualificationStatus));
        }

        if (request.Profession is { } profession)
        {
            profile.UpdateProfession(Profession.Create(
                profession.JobStatus ?? profile.Profession.JobStatus,
                profession.Occupation ?? profile.Profession.Occupation,
                profession.IncomeRange ?? profile.Profession.IncomeRange));
        }

        if (request.Residency is { } residency)
        {
            profile.UpdateResidency(Residency.Create(
                residency.City ?? profile.Residency.City,
                residency.District ?? profile.Residency.District,
                residency.Country ?? profile.Residency.Country));
        }

        if (request.Family is { } family)
        {
            var updatedFamily = Family.Create(
                family.FatherOccupation ?? profile.Family.FatherOccupation,
                family.MotherOccupation ?? profile.Family.MotherOccupation,
                family.SiblingCount ?? profile.Family.SiblingCount);

            var siblings = family.Siblings?.Select(s => (s.Relationship, s.MaritalStatus, s.Occupation))
                ?? profile.Siblings.Select(s => (s.Relationship, s.MaritalStatus, s.Occupation));

            profile.UpdateFamily(updatedFamily, siblings);
        }

        if (request.Lifestyle is { } lifestyle)
        {
            profile.UpdateLifestyle(Lifestyle.Create(
                lifestyle.Smoking ?? profile.Lifestyle.Smoking,
                lifestyle.Alcohol ?? profile.Lifestyle.Alcohol));
        }

        if (request.Assets is { } assets)
        {
            profile.UpdateAssets(Assets.Create(assets.Status ?? profile.Assets.Status));
        }

        if (request.Photos is { } photos)
        {
            profile.ReplacePhotos(photos.Select(p => (p.Uri, p.IsBlurred)));
        }

        await _dbContext.SaveChangesAsync();

        return Result.Success(await ToResponseAsync(profile, userId));
    }

    public async Task<Result<PhotoUploadResponse>> UploadPhotoAsync(Guid userId, IFormFile file)
    {
        if (file.Length == 0)
        {
            return Result.Failure<PhotoUploadResponse>("The uploaded file is empty.", "EMPTY_FILE");
        }

        if (file.Length > MaxPhotoBytes)
        {
            return Result.Failure<PhotoUploadResponse>("Photos must be 5MB or smaller.", "FILE_TOO_LARGE");
        }

        if (!AllowedPhotoContentTypes.Contains(file.ContentType))
        {
            return Result.Failure<PhotoUploadResponse>("Only JPEG, PNG, or WebP photos are allowed.", "UNSUPPORTED_FILE_TYPE");
        }

        var extension = file.ContentType switch
        {
            "image/png" => ".png",
            "image/webp" => ".webp",
            _ => ".jpg"
        };
        var key = $"profiles/{userId}/{Guid.NewGuid()}{extension}";

        await using var stream = file.OpenReadStream();
        var uri = await _objectStorage.UploadAsync(stream, key, file.ContentType);

        return Result.Success(new PhotoUploadResponse(uri));
    }

    public async Task<Result<ProfileResponse>> SendConnectionRequestAsync(Guid profileId, Guid requesterUserId)
    {
        var target = await LoadProfileAsync(p => p.Id == profileId);
        if (target is null)
        {
            return Result.Failure<ProfileResponse>("This profile could not be found.", "PROFILE_NOT_FOUND");
        }

        if (target.UserId == requesterUserId)
        {
            return Result.Failure<ProfileResponse>("You cannot send a request to yourself.", "INVALID_TARGET");
        }

        var forward = await _dbContext.ProfileConnectionRequests
            .FirstOrDefaultAsync(r => r.RequesterUserId == requesterUserId && r.TargetUserId == target.UserId);
        var reverse = await _dbContext.ProfileConnectionRequests
            .FirstOrDefaultAsync(r => r.RequesterUserId == target.UserId && r.TargetUserId == requesterUserId);

        if (reverse is { Status: ConnectionStatus.Sent })
        {
            // They already sent us a request — sending one back completes a mutual match.
            reverse.Accept();
        }
        else if (forward is null)
        {
            _dbContext.ProfileConnectionRequests.Add(ProfileConnectionRequest.Create(requesterUserId, target.UserId));
        }
        else if (forward.Status == ConnectionStatus.Declined)
        {
            if (forward.IsBlocked)
            {
                return Result.Failure<ProfileResponse>(
                    "This profile has declined your request too many times. You can no longer send them a request.",
                    "REQUEST_BLOCKED");
            }

            // Reuses the same row (the unique index allows only one per requester/target pair)
            // so DeclineCount keeps accumulating across resends.
            forward.Resend();
        }

        await _dbContext.SaveChangesAsync();
        return Result.Success(await ToResponseAsync(target, requesterUserId));
    }

    public async Task<Result<ProfileResponse>> CancelConnectionRequestAsync(Guid profileId, Guid requesterUserId)
    {
        var target = await LoadProfileAsync(p => p.Id == profileId);
        if (target is null)
        {
            return Result.Failure<ProfileResponse>("This profile could not be found.", "PROFILE_NOT_FOUND");
        }

        var forward = await _dbContext.ProfileConnectionRequests
            .FirstOrDefaultAsync(r => r.RequesterUserId == requesterUserId && r.TargetUserId == target.UserId && r.Status == ConnectionStatus.Sent);
        if (forward is not null)
        {
            _dbContext.ProfileConnectionRequests.Remove(forward);
            await _dbContext.SaveChangesAsync();
        }

        return Result.Success(await ToResponseAsync(target, requesterUserId));
    }

    public async Task<Result<ProfileResponse>> ToggleFavouriteAsync(Guid profileId, Guid userId)
    {
        var target = await LoadProfileAsync(p => p.Id == profileId);
        if (target is null)
        {
            return Result.Failure<ProfileResponse>("This profile could not be found.", "PROFILE_NOT_FOUND");
        }

        var existing = await _dbContext.ProfileFavourites
            .FirstOrDefaultAsync(f => f.UserId == userId && f.TargetProfileId == profileId);

        if (existing is not null)
        {
            _dbContext.ProfileFavourites.Remove(existing);
        }
        else
        {
            _dbContext.ProfileFavourites.Add(ProfileFavourite.Create(userId, profileId));
        }

        await _dbContext.SaveChangesAsync();
        return Result.Success(await ToResponseAsync(target, userId));
    }

    public async Task<Result<IReadOnlyList<ConnectionRequestResponse>>> GetSentRequestsAsync(Guid userId)
    {
        var requests = await _dbContext.ProfileConnectionRequests
            .Where(r => r.RequesterUserId == userId)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync();

        var responses = new List<ConnectionRequestResponse>();
        foreach (var request in requests)
        {
            var otherParty = await LoadProfileAsync(p => p.UserId == request.TargetUserId);
            if (otherParty is null) continue;

            responses.Add(new ConnectionRequestResponse(
                request.Id,
                ToSummary(otherParty),
                MapSentStatus(request),
                request.CreatedAt));
        }

        return Result.Success<IReadOnlyList<ConnectionRequestResponse>>(responses);
    }

    public async Task<Result<IReadOnlyList<ConnectionRequestResponse>>> GetReceivedRequestsAsync(Guid userId)
    {
        var requests = await _dbContext.ProfileConnectionRequests
            .Where(r => r.TargetUserId == userId)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync();

        var responses = new List<ConnectionRequestResponse>();
        foreach (var request in requests)
        {
            var otherParty = await LoadProfileAsync(p => p.UserId == request.RequesterUserId);
            if (otherParty is null) continue;

            responses.Add(new ConnectionRequestResponse(
                request.Id,
                ToSummary(otherParty),
                MapReceivedStatus(request.Status),
                request.CreatedAt));
        }

        return Result.Success<IReadOnlyList<ConnectionRequestResponse>>(responses);
    }

    public async Task<Result<ConnectionRequestResponse>> AcceptRequestAsync(Guid requestId, Guid userId)
    {
        var request = await _dbContext.ProfileConnectionRequests.FirstOrDefaultAsync(r => r.Id == requestId);
        if (request is null)
        {
            return Result.Failure<ConnectionRequestResponse>("This request could not be found.", "REQUEST_NOT_FOUND");
        }

        if (request.TargetUserId != userId)
        {
            return Result.Failure<ConnectionRequestResponse>("You cannot respond to this request.", "FORBIDDEN");
        }

        request.Accept();
        await _dbContext.SaveChangesAsync();

        var otherParty = await LoadProfileAsync(p => p.UserId == request.RequesterUserId);
        if (otherParty is null)
        {
            return Result.Failure<ConnectionRequestResponse>("This profile could not be found.", "PROFILE_NOT_FOUND");
        }

        return Result.Success(new ConnectionRequestResponse(
            request.Id, ToSummary(otherParty), MapReceivedStatus(request.Status), request.CreatedAt));
    }

    public async Task<Result<ConnectionRequestResponse>> DeclineRequestAsync(Guid requestId, Guid userId)
    {
        var request = await _dbContext.ProfileConnectionRequests.FirstOrDefaultAsync(r => r.Id == requestId);
        if (request is null)
        {
            return Result.Failure<ConnectionRequestResponse>("This request could not be found.", "REQUEST_NOT_FOUND");
        }

        if (request.TargetUserId != userId)
        {
            return Result.Failure<ConnectionRequestResponse>("You cannot respond to this request.", "FORBIDDEN");
        }

        request.Decline();
        await _dbContext.SaveChangesAsync();

        var otherParty = await LoadProfileAsync(p => p.UserId == request.RequesterUserId);
        if (otherParty is null)
        {
            return Result.Failure<ConnectionRequestResponse>("This profile could not be found.", "PROFILE_NOT_FOUND");
        }

        return Result.Success(new ConnectionRequestResponse(
            request.Id, ToSummary(otherParty), MapReceivedStatus(request.Status), request.CreatedAt));
    }

    public async Task<Result<IReadOnlyList<ProfileSummaryResponse>>> GetFavouritesAsync(Guid userId)
    {
        var favourites = await _dbContext.ProfileFavourites
            .Where(f => f.UserId == userId)
            .OrderByDescending(f => f.CreatedAt)
            .ToListAsync();

        var responses = new List<ProfileSummaryResponse>();
        foreach (var favourite in favourites)
        {
            var target = await LoadProfileAsync(p => p.Id == favourite.TargetProfileId);
            if (target is null) continue;

            responses.Add(ToSummary(target));
        }

        return Result.Success<IReadOnlyList<ProfileSummaryResponse>>(responses);
    }

    private static string MapSentStatus(ProfileConnectionRequest request) => request.Status switch
    {
        ConnectionStatus.Accepted => "accepted",
        ConnectionStatus.Declined when request.IsBlocked => "blocked",
        ConnectionStatus.Declined => "declined",
        _ => "sent"
    };

    private static string MapReceivedStatus(ConnectionStatus status) => status switch
    {
        ConnectionStatus.Accepted => "accepted",
        ConnectionStatus.Declined => "declined",
        _ => "pending"
    };

    private static ProfileSummaryResponse ToSummary(Profile profile) => new(
        profile.Id,
        profile.FirstName,
        profile.LastName,
        profile.Photos.OrderBy(p => p.Order).Select(p => p.Uri).FirstOrDefault(),
        profile.CalculateAge(),
        profile.Residency.City,
        FormatLastActiveLabel(profile.UpdatedAt),
        profile.IsGold);

    private Task<Profile?> LoadProfileAsync(System.Linq.Expressions.Expression<Func<Profile, bool>> predicate) =>
        _dbContext.Profiles
            .Include(p => p.Siblings)
            .Include(p => p.Photos)
            .FirstOrDefaultAsync(predicate);

    private async Task<string> GetRelationshipStatusAsync(Guid viewerUserId, Guid targetUserId)
    {
        if (viewerUserId == targetUserId)
        {
            return "none";
        }

        var forward = await _dbContext.ProfileConnectionRequests
            .FirstOrDefaultAsync(r => r.RequesterUserId == viewerUserId && r.TargetUserId == targetUserId);
        if (forward is not null)
        {
            return forward.Status switch
            {
                ConnectionStatus.Accepted => "connected",
                ConnectionStatus.Declined when forward.IsBlocked => "blocked",
                ConnectionStatus.Declined => "declined",
                _ => "sent"
            };
        }

        var reverse = await _dbContext.ProfileConnectionRequests
            .FirstOrDefaultAsync(r => r.RequesterUserId == targetUserId && r.TargetUserId == viewerUserId);
        if (reverse is not null)
        {
            return reverse.Status switch
            {
                ConnectionStatus.Accepted => "connected",
                ConnectionStatus.Declined => "declined",
                _ => "pending"
            };
        }

        return "none";
    }

    private async Task<ProfileResponse> ToResponseAsync(Profile profile, Guid? viewerUserId)
    {
        var isOwnProfile = viewerUserId == profile.UserId;

        var isFavourite = viewerUserId.HasValue && !isOwnProfile && await _dbContext.ProfileFavourites
            .AnyAsync(f => f.UserId == viewerUserId.Value && f.TargetProfileId == profile.Id);
        var relationshipStatus = viewerUserId.HasValue
            ? await GetRelationshipStatusAsync(viewerUserId.Value, profile.UserId)
            : "none";

        var owner = await _userManager.FindByIdAsync(profile.UserId.ToString());
        var emailVerified = owner?.EmailConfirmed ?? false;

        return new ProfileResponse(
            profile.Id,
            profile.UserId,
            profile.FirstName,
            profile.LastName,
            profile.Photos.OrderBy(p => p.Order).Select(p => new PhotoResponse(p.Uri, p.IsBlurred)).ToList(),
            profile.CalculateAge(),
            profile.Residency.City,
            FormatLastActiveLabel(profile.UpdatedAt),
            profile.ManagedBy,
            isFavourite,
            relationshipStatus,
            new BasicInfoResponse(
                profile.BasicInfo.Gender,
                profile.BasicInfo.Race,
                profile.BasicInfo.Religion,
                profile.BasicInfo.Caste,
                profile.CalculateMaskedDateOfBirth(),
                profile.BasicInfo.MaritalStatus,
                profile.BasicInfo.HeightLabel),
            new EducationResponse(profile.Education.Qualification, profile.Education.QualificationStatus),
            new ProfessionResponse(profile.Profession.JobStatus, profile.Profession.Occupation, profile.Profession.IncomeRange),
            new ResidencyResponse(profile.Residency.City, profile.Residency.District, profile.Residency.Country),
            new FamilyResponse(
                profile.Family.FatherOccupation,
                profile.Family.MotherOccupation,
                profile.Family.SiblingCount,
                profile.Siblings.Select(s => new SiblingResponse(s.Relationship, s.MaritalStatus, s.Occupation)).ToList()),
            new LifestyleResponse(profile.Lifestyle.Smoking, profile.Lifestyle.Alcohol),
            new AssetsResponse(profile.Assets.Status),
            profile.Hobbies,
            profile.Interests,
            new VerificationResponse(
                profile.Verification.PhoneVerified,
                emailVerified,
                profile.Verification.IdentityVerified,
                profile.Verification.PhotoVerified,
                profile.Verification.ProfessionVerified,
                profile.Verification.EducationVerified),
            profile.AboutMe,
            profile.IsGold);
    }

    private static string FormatLastActiveLabel(DateTime lastActiveAt)
    {
        var elapsed = DateTime.UtcNow - lastActiveAt;
        if (elapsed <= TimeSpan.FromMinutes(5)) return "Online now";
        if (elapsed < TimeSpan.FromHours(1)) return $"Active {(int)elapsed.TotalMinutes}m ago";
        if (elapsed < TimeSpan.FromDays(1)) return $"Active {(int)elapsed.TotalHours}h ago";
        return $"Active {(int)elapsed.TotalDays}d ago";
    }
}
