using Wedding_Proposal_BE.Features.Users.Application.DTOs;
using Wedding_Proposal_BE.Shared.Common;

namespace Wedding_Proposal_BE.Features.Users.Application.Interfaces;

public interface IUserService
{
    Task<Result<UserResponse>> GetByIdAsync(Guid userId);
}
