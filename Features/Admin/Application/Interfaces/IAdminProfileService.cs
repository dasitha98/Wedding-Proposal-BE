using Wedding_Proposal_BE.Features.Admin.Application.DTOs;
using Wedding_Proposal_BE.Shared.Common;

namespace Wedding_Proposal_BE.Features.Admin.Application.Interfaces;

public interface IAdminProfileService
{
    Task<Result<AdminPagedResponse<AdminProfileSummaryDto>>> ListAsync(string? search, int page, int pageSize);
    Task<Result<AdminProfileDetailDto>> GetByIdAsync(Guid id);
    Task<Result<AdminProfileDetailDto>> CreateAsync(AdminCreateProfileRequest request);
    Task<Result<AdminProfileDetailDto>> UpdateAsync(Guid id, AdminUpdateProfileRequest request);
    Task<Result> DeleteAsync(Guid id);
    Task<Result> DeletePhotoAsync(Guid profileId, Guid photoId);
}
