namespace Wedding_Proposal_BE.Features.Auth.Infrastructure.Services;

/// The verified identity extracted from a Google ID token. `Subject` (the `sub` claim) is
/// Google's stable, unique identifier for the end user — the only field ever used to look up
/// or create an account.
public record GoogleIdentity(string Subject, string Email, bool EmailVerified, string? Name, string? PictureUrl);

/// Thrown for any failure while exchanging the authorization code or validating the resulting
/// ID token (bad/expired code, signature failure, wrong issuer/audience, expired token, nonce
/// mismatch). Deliberately doesn't distinguish the reason in its public message — none of these
/// failure modes should be disclosed to the client beyond "Google sign-in failed."
public class GoogleOidcException : Exception
{
    public GoogleOidcException(string message, Exception? inner = null) : base(message, inner)
    {
    }
}

/// Talks to Google's OIDC endpoints. Isolated behind an interface so AuthService's use-case
/// logic can be unit tested without making real network calls to Google.
public interface IGoogleOidcClient
{
    /// Exchanges an authorization code (+ PKCE verifier) for tokens, then validates the
    /// resulting ID token's signature, issuer, audience, expiry, and nonce before returning the
    /// identity it carries. Only the ID token is used; any access/refresh token Google returns
    /// is discarded immediately — the app never stores or reuses Google's own tokens.
    Task<GoogleIdentity> AuthenticateAsync(
        string code, string codeVerifier, string redirectUri, string expectedNonce, CancellationToken cancellationToken);
}
