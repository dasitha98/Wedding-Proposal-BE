using Wedding_Proposal_BE.Features.Admin.Application.DTOs;
using Wedding_Proposal_BE.Shared.Common;

namespace Wedding_Proposal_BE.Features.Admin.Application.Interfaces;

public interface IAdminSecurityService
{
    Task<Result<AdminPagedResponse<AdminRefreshTokenDto>>> ListRefreshTokensAsync(int page, int pageSize);
    Task<Result> DeleteRefreshTokenAsync(Guid id);

    Task<Result<AdminPagedResponse<AdminOtpDto>>> ListPasswordResetOtpsAsync(int page, int pageSize);
    Task<Result> DeletePasswordResetOtpAsync(Guid id);

    Task<Result<AdminPagedResponse<AdminOtpDto>>> ListEmailVerificationOtpsAsync(int page, int pageSize);
    Task<Result> DeleteEmailVerificationOtpAsync(Guid id);
}
