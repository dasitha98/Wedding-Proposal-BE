using Wedding_Proposal_BE.Features.Admin.Application.DTOs;
using Wedding_Proposal_BE.Shared.Common;

namespace Wedding_Proposal_BE.Features.Admin.Application.Interfaces;

public interface IAdminUserService
{
    Task<Result<AdminPagedResponse<AdminUserDto>>> ListAsync(string? search, int page, int pageSize);
    Task<Result<AdminUserDto>> GetByIdAsync(Guid id);
    Task<Result<AdminUserDto>> CreateAsync(AdminCreateUserRequest request);
    Task<Result<AdminUserDto>> UpdateAsync(Guid id, AdminUpdateUserRequest request);
    Task<Result> DeleteAsync(Guid id);
}
