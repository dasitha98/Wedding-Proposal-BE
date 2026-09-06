using Wedding_Proposal_BE.Features.Auth.Application.DTOs;
using Wedding_Proposal_BE.Shared.Common;

namespace Wedding_Proposal_BE.Features.Auth.Application.Interfaces;

/// Orchestrates the register/login/refresh/revoke use cases. Expected failures (wrong
/// password, duplicate email, expired refresh token) are returned as Result failures rather
/// than thrown — exceptions are reserved for truly exceptional situations.
public interface IAuthService
{
    /// Creates the (unverified) account and emails an OTP; no auth tokens are issued until
    /// VerifyRegistrationOtpAsync succeeds.
    Task<Result<RegisterResponse>> RegisterAsync(RegisterRequest request);

    Task<Result<AuthResponse>> VerifyRegistrationOtpAsync(string email, string code);

    Task<Result<RegisterResponse>> ResendRegistrationOtpAsync(string email);

    Task<Result<AuthResponse>> LoginAsync(LoginRequest request);

    Task<Result<AuthResponse>> RefreshAsync(string refreshToken);

    Task<Result> RevokeAsync(string refreshToken);

    Task<Result<RequestPasswordResetOtpResponse>> RequestPasswordResetOtpAsync(string email);

    Task<Result<VerifyPasswordResetOtpResponse>> VerifyPasswordResetOtpAsync(string email, string code);

    Task<Result> ResetPasswordAsync(string resetToken, string newPassword);

    Task<Result> ChangePasswordAsync(Guid userId, string currentPassword, string newPassword);

    Task<Result<AuthUserResponse>> UpdateAccountAsync(Guid userId, UpdateAccountRequest request);

    /// Begins a Google sign-in: generates and stores (server-side only) the PKCE verifier and
    /// nonce for this attempt, and returns the Google authorization URL to open plus the
    /// opaque `state` that identifies the pending attempt.
    Task<Result<GoogleAuthStartResponse>> StartGoogleSignInAsync(string redirectUri);

    /// Completes a Google sign-in: validates `state` (CSRF protection), exchanges `code` for
    /// tokens using the PKCE verifier stored under that state, validates the resulting ID token
    /// (signature/issuer/audience/expiry/nonce), and finds-or-creates the user by Google
    /// `sub`.
    Task<Result<AuthResponse>> CompleteGoogleSignInAsync(string code, string state);
}
