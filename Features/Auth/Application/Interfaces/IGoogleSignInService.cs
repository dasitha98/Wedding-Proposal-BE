using Wedding_Proposal_BE.Features.Auth.Application.DTOs;
using Wedding_Proposal_BE.Shared.Common;

namespace Wedding_Proposal_BE.Features.Auth.Application.Interfaces;

/// Owns the Google OIDC sign-in flow (PKCE + state). Expected failures are returned as
/// Result failures rather than thrown.
public interface IGoogleSignInService
{
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
