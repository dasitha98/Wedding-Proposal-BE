using Wedding_Proposal_BE.Features.Auth.Application.DTOs;
using Wedding_Proposal_BE.Shared.Infrastructure.Identity;

namespace Wedding_Proposal_BE.Features.Auth.Application.Interfaces;

/// Shared by every use case that hands back an authenticated session (registration
/// verification, login, Google sign-in) so token issuance and refresh-token persistence
/// live in one place instead of being duplicated per use case.
public interface IAuthResponseFactory
{
    /// Issues a new access + refresh token pair for the user and persists the refresh token.
    Task<AuthResponse> IssueAsync(ApplicationUser user);

    Task<AuthUserResponse> ToUserResponseAsync(ApplicationUser user);
}
