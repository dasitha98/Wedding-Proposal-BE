namespace Wedding_Proposal_BE.Features.Auth.Infrastructure.Options;

public class GoogleAuthOptions
{
    public const string SectionName = "Google";

    /// OAuth 2.0 client ID from the Google Cloud Console (OAuth consent screen). This is the
    /// only value the app's ID token audience is ever validated against.
    public string ClientId { get; set; } = string.Empty;

    /// OAuth 2.0 client secret for the same client. Never sent to the frontend — only used
    /// server-side to authenticate the authorization-code exchange with Google. Keep this out
    /// of appsettings.json; set it via user-secrets locally and an env var / secret manager in
    /// production (see Google:ClientSecret in appsettings.json's comment).
    public string ClientSecret { get; set; } = string.Empty;

    /// Redirect URIs the backend will accept from the frontend when starting a Google sign-in,
    /// matched by exact value or (if an entry ends with "://") by scheme prefix. Prevents an
    /// attacker from redirecting the authorization code to a URI they control (open-redirect /
    /// code-theft protection) — every redirect URI the app can legitimately use must be listed
    /// here explicitly.
    public string[] AllowedRedirectUris { get; set; } = [];
}
