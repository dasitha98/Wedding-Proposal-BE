using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Wedding_Proposal_BE.Features.Admin.Application.DTOs;
using Wedding_Proposal_BE.Features.Admin.Application.Interfaces;
using Wedding_Proposal_BE.Features.Profiles.Domain.Entities;
using Wedding_Proposal_BE.Features.Profiles.Domain.ValueObjects;
using Wedding_Proposal_BE.Shared.Common;
using Wedding_Proposal_BE.Shared.Infrastructure.Data;
using Wedding_Proposal_BE.Shared.Infrastructure.Identity;

namespace Wedding_Proposal_BE.Features.Admin.Infrastructure.Services;

public class AdminProfileService : IAdminProfileService
{
    private readonly AppDbContext _dbContext;
    private readonly UserManager<ApplicationUser> _userManager;

    public AdminProfileService(AppDbContext dbContext, UserManager<ApplicationUser> userManager)
    {
        _dbContext = dbContext;
        _userManager = userManager;
    }

    public async Task<Result<AdminPagedResponse<AdminProfileSummaryDto>>> ListAsync(string? search, int page, int pageSize)
    {
        var query = _dbContext.Profiles.Include(p => p.Photos).AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim().ToLower();
            query = query.Where(p => p.FirstName.ToLower().Contains(term) || p.LastName.ToLower().Contains(term));
        }

        query = query.OrderByDescending(p => p.CreatedAt);

        var paged = await AdminPaging.ToPagedAsync(query, page, pageSize, p => p);

        var userIds = paged.Items.Select(p => p.UserId).ToList();
        var emailLookup = await _dbContext.Users
            .Where(u => userIds.Contains(u.Id))
            .ToDictionaryAsync(u => u.Id, u => u.Email ?? string.Empty);

        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var dtos = paged.Items.Select(p => new AdminProfileSummaryDto(
            p.Id, p.UserId, $"{p.FirstName} {p.LastName}".Trim(),
            emailLookup.TryGetValue(p.UserId, out var email) ? email : string.Empty,
            p.BasicInfo.Gender, p.CalculateAge(today), p.BasicInfo.Religion,
            p.Residency.City, p.Residency.Country, p.IsGold, p.Photos.Count, p.CreatedAt)).ToList();

        return Result.Success(new AdminPagedResponse<AdminProfileSummaryDto>(dtos, paged.TotalCount, paged.TotalPages, paged.Page, paged.PageSize));
    }

    public async Task<Result<AdminProfileDetailDto>> GetByIdAsync(Guid id)
    {
        var profile = await _dbContext.Profiles
            .Include(p => p.Photos)
            .Include(p => p.Siblings)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (profile is null)
        {
            return Result.Failure<AdminProfileDetailDto>("Profile not found.", "PROFILE_NOT_FOUND");
        }

        var user = await _userManager.FindByIdAsync(profile.UserId.ToString());
        return Result.Success(ToDetailDto(profile, user?.Email ?? string.Empty));
    }

    public async Task<Result<AdminProfileDetailDto>> CreateAsync(AdminCreateProfileRequest request)
    {
        var user = await _userManager.FindByIdAsync(request.UserId.ToString());
        if (user is null)
        {
            return Result.Failure<AdminProfileDetailDto>("User not found.", "USER_NOT_FOUND");
        }

        var existing = await _dbContext.Profiles.FirstOrDefaultAsync(p => p.UserId == request.UserId);
        if (existing is not null)
        {
            return Result.Failure<AdminProfileDetailDto>("This user already has a profile.", "PROFILE_EXISTS");
        }

        var profile = Profile.CreateBlank(request.UserId, request.FirstName.Trim(), request.LastName.Trim(), request.DateOfBirth);
        _dbContext.Profiles.Add(profile);
        await _dbContext.SaveChangesAsync();

        return Result.Success(ToDetailDto(profile, user.Email ?? string.Empty));
    }

    public async Task<Result<AdminProfileDetailDto>> UpdateAsync(Guid id, AdminUpdateProfileRequest request)
    {
        var profile = await _dbContext.Profiles
            .Include(p => p.Photos)
            .Include(p => p.Siblings)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (profile is null)
        {
            return Result.Failure<AdminProfileDetailDto>("Profile not found.", "PROFILE_NOT_FOUND");
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

        if (request.IsGold.HasValue)
        {
            profile.SetGoldStatus(request.IsGold.Value);
        }

        if (request.Verification is not null)
        {
            var v = request.Verification;
            profile.UpdateVerification(Verification.Create(
                v.PhoneVerified, v.EmailVerified, v.IdentityVerified, v.PhotoVerified, v.ProfessionVerified, v.EducationVerified));
        }

        await _dbContext.SaveChangesAsync();

        var user = await _userManager.FindByIdAsync(profile.UserId.ToString());
        return Result.Success(ToDetailDto(profile, user?.Email ?? string.Empty));
    }

    public async Task<Result> DeleteAsync(Guid id)
    {
        var profile = await _dbContext.Profiles.FirstOrDefaultAsync(p => p.Id == id);
        if (profile is null)
        {
            return Result.Failure("Profile not found.", "PROFILE_NOT_FOUND");
        }

        _dbContext.Profiles.Remove(profile);
        await _dbContext.SaveChangesAsync();
        return Result.Success();
    }

    public async Task<Result> DeletePhotoAsync(Guid profileId, Guid photoId)
    {
        var profile = await _dbContext.Profiles.Include(p => p.Photos).FirstOrDefaultAsync(p => p.Id == profileId);
        if (profile is null)
        {
            return Result.Failure("Profile not found.", "PROFILE_NOT_FOUND");
        }

        if (profile.Photos.All(ph => ph.Id != photoId))
        {
            return Result.Failure("Photo not found.", "PHOTO_NOT_FOUND");
        }

        profile.ReplacePhotos(profile.Photos.Where(ph => ph.Id != photoId).OrderBy(ph => ph.Order).Select(ph => (ph.Uri, ph.IsBlurred)));
        await _dbContext.SaveChangesAsync();
        return Result.Success();
    }

    private static AdminProfileDetailDto ToDetailDto(Profile p, string email) => new(
        p.Id, p.UserId, p.FirstName, p.LastName, email, p.DateOfBirth, p.CalculateAge(),
        p.ManagedBy, p.AboutMe, p.Hobbies, p.Interests, p.IsGold, p.CreatedAt, p.UpdatedAt,
        new AdminBasicInfoDto(p.BasicInfo.Gender, p.BasicInfo.Race, p.BasicInfo.Religion, p.BasicInfo.Caste, p.BasicInfo.MaritalStatus, p.BasicInfo.HeightLabel),
        new AdminEducationDto(p.Education.Qualification, p.Education.QualificationStatus),
        new AdminProfessionDto(p.Profession.JobStatus, p.Profession.Occupation, p.Profession.IncomeRange),
        new AdminResidencyDto(p.Residency.City, p.Residency.District, p.Residency.Country),
        new AdminFamilyDto(p.Family.FatherOccupation, p.Family.MotherOccupation, p.Family.SiblingCount),
        new AdminLifestyleDto(p.Lifestyle.Smoking, p.Lifestyle.Alcohol),
        new AdminAssetsDto(p.Assets.Status),
        new AdminVerificationDto(
            p.Verification.PhoneVerified, p.Verification.EmailVerified, p.Verification.IdentityVerified,
            p.Verification.PhotoVerified, p.Verification.ProfessionVerified, p.Verification.EducationVerified),
        p.Photos.OrderBy(ph => ph.Order).Select(ph => new AdminPhotoDto(ph.Id, ph.Uri, ph.IsBlurred, ph.Order)).ToList(),
        p.Siblings.Select(s => new AdminSiblingDto(s.Id, s.Relationship, s.MaritalStatus, s.Occupation)).ToList());
}
