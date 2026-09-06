using Wedding_Proposal_BE.Shared.Infrastructure.Identity;

namespace Wedding_Proposal_BE.Features.Auth.Application.Interfaces;

/// Narrow, purpose-specific interface (Interface Segregation) so AuthService depends only
/// on token creation/validation, not on the whole auth surface. Also the Strategy Pattern
/// seam: a future opaque-token or session-token strategy can implement this without
/// AuthService changing. Wrapping an implementation with a logging/caching decorator is a
/// natural future extension of this interface (Decorator Pattern) — no changes needed here.
public interface ITokenService
{
    (string Token, DateTime ExpiresAt) CreateAccessToken(ApplicationUser user, IEnumerable<string> roles);

    string GenerateRefreshTokenValue();

    Guid? ValidateAccessTokenAndGetUserId(string accessToken);

    /// Short-lived, single-purpose token authorizing one password reset for the given email.
    string CreateResetToken(string email, TimeSpan lifetime);

    string? ValidateResetTokenAndGetEmail(string resetToken);
}
