using Wedding_Proposal_BE.Features.Auth.Application.DTOs;
using Wedding_Proposal_BE.Shared.Common;

namespace Wedding_Proposal_BE.Features.Auth.Application.Interfaces;

/// Owns the authenticated-session lifecycle: password login, refresh-token rotation, and
/// revocation. Expected failures are returned as Result failures rather than thrown.
public interface ISessionService
{
    Task<Result<AuthResponse>> LoginAsync(LoginRequest request);

    Task<Result<AuthResponse>> RefreshAsync(string refreshToken);

    Task<Result> RevokeAsync(string refreshToken);
}
