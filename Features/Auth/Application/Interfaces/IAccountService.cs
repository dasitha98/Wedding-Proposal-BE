using Wedding_Proposal_BE.Features.Auth.Application.DTOs;
using Wedding_Proposal_BE.Shared.Common;

namespace Wedding_Proposal_BE.Features.Auth.Application.Interfaces;

/// Owns updates to the identity-level account fields (name/email) that Auth itself is
/// responsible for. Expected failures are returned as Result failures rather than thrown.
public interface IAccountService
{
    Task<Result<AuthUserResponse>> UpdateAccountAsync(Guid userId, UpdateAccountRequest request);
}
