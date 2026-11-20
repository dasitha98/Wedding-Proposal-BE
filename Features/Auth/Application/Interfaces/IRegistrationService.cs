using Wedding_Proposal_BE.Features.Auth.Application.DTOs;
using Wedding_Proposal_BE.Shared.Common;

namespace Wedding_Proposal_BE.Features.Auth.Application.Interfaces;

/// Owns the sign-up flow: create the (unverified) account, email an OTP, and verify it.
/// Expected failures are returned as Result failures rather than thrown.
public interface IRegistrationService
{
    /// Creates the (unverified) account and emails an OTP; no auth tokens are issued until
    /// VerifyRegistrationOtpAsync succeeds.
    Task<Result<RegisterResponse>> RegisterAsync(RegisterRequest request);

    Task<Result<AuthResponse>> VerifyRegistrationOtpAsync(string email, string code);

    Task<Result<RegisterResponse>> ResendRegistrationOtpAsync(string email);
}
