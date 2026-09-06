using Microsoft.AspNetCore.Http;
using Wedding_Proposal_BE.Features.Profiles.Application.DTOs;
using Wedding_Proposal_BE.Shared.Common;

namespace Wedding_Proposal_BE.Features.Profiles.Application.Interfaces;

public interface IProfileService
{
    Task<Result<ProfileResponse>> GetByUserIdAsync(Guid userId);

    Task<Result<ProfileResponse>> GetByIdAsync(Guid profileId, Guid? viewerUserId);

    Task<Result<ProfileResponse>> CreateAsync(Guid userId, CreateProfileRequest request);

    Task<Result<ProfileResponse>> UpdateAsync(Guid userId, UpdateProfileRequest request);

    Task<Result<PhotoUploadResponse>> UploadPhotoAsync(Guid userId, IFormFile file);

    Task<Result<ProfileResponse>> SendConnectionRequestAsync(Guid profileId, Guid requesterUserId);

    Task<Result<ProfileResponse>> CancelConnectionRequestAsync(Guid profileId, Guid requesterUserId);

    Task<Result<ProfileResponse>> ToggleFavouriteAsync(Guid profileId, Guid userId);

    Task<Result<IReadOnlyList<ConnectionRequestResponse>>> GetSentRequestsAsync(Guid userId);

    Task<Result<IReadOnlyList<ConnectionRequestResponse>>> GetReceivedRequestsAsync(Guid userId);

    Task<Result<ConnectionRequestResponse>> AcceptRequestAsync(Guid requestId, Guid userId);

    Task<Result<ConnectionRequestResponse>> DeclineRequestAsync(Guid requestId, Guid userId);

    Task<Result<IReadOnlyList<ProfileSummaryResponse>>> GetFavouritesAsync(Guid userId);
}
