namespace Wedding_Proposal_BE.Features.Auth.Infrastructure.Services;

/// One pending "sign in with Google" attempt: the PKCE code verifier and nonce the backend
/// generated when it built the authorization URL, plus the redirect URI it was bound to.
/// Never leaves the server — the frontend only ever sees the resulting `state`.
public record GoogleOidcTransaction(string CodeVerifier, string Nonce, string RedirectUri);

/// Server-side store for in-flight Google sign-in attempts, keyed by the opaque `state` value
/// returned to the frontend. Provides the CSRF protection in the flow: a callback can only
/// succeed if it presents a `state` the backend itself issued and hasn't already consumed.
public interface IGoogleOidcTransactionStore
{
    /// Stores a new transaction and returns the `state` value that identifies it.
    string Create(string codeVerifier, string nonce, string redirectUri);

    /// Looks up and immediately deletes the transaction for `state` (single use — replaying
    /// the same state twice always fails on the second attempt). Returns false if `state` is
    /// unknown or has expired.
    bool TryConsume(string state, out GoogleOidcTransaction transaction);
}
