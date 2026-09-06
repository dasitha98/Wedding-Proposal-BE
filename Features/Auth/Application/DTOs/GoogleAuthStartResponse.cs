namespace Wedding_Proposal_BE.Features.Auth.Application.DTOs;

/// `AuthorizationUrl` is the full Google OIDC authorization endpoint URL, already carrying the
/// client ID, redirect URI, scopes, PKCE code challenge, nonce, and `state` — the frontend just
/// opens it in a browser/web-auth session and doesn't need to know any of those details. `State`
/// is returned separately purely so the frontend can sanity-check it against the value Google
/// echoes back on redirect before ever calling the backend again.
public record GoogleAuthStartResponse(string AuthorizationUrl, string State);
