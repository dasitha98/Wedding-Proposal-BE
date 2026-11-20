using Wedding_Proposal_BE.Features.Auth.Application.DTOs;
using Wedding_Proposal_BE.Shared.Common;

namespace Wedding_Proposal_BE.Features.Auth.Application.Interfaces;

/// Owns password management: the OTP-gated forgot-password flow and change-password for an
/// already-authenticated user. Expected failures are returned as Result failures rather than
/// thrown.
public interface IPasswordService
{
    Task<Result<RequestPasswordResetOtpResponse>> RequestPasswordResetOtpAsync(string email);

    Task<Result<VerifyPasswordResetOtpResponse>> VerifyPasswordResetOtpAsync(string email, string code);

    Task<Result> ResetPasswordAsync(string resetToken, string newPassword);

    Task<Result> ChangePasswordAsync(Guid userId, string currentPassword, string newPassword);
}
