using Microsoft.AspNetCore.Identity;
using Wedding_Proposal_BE.Features.Users.Application.DTOs;
using Wedding_Proposal_BE.Features.Users.Application.Interfaces;
using Wedding_Proposal_BE.Shared.Common;
using Wedding_Proposal_BE.Shared.Infrastructure.Identity;

namespace Wedding_Proposal_BE.Features.Users.Infrastructure.Services;

/// Reads user data via UserManager<T> (Identity already serves as the repository here) —
/// no custom repository abstraction is introduced over it.
public class UserService : IUserService
{
    private readonly UserManager<ApplicationUser> _userManager;

    public UserService(UserManager<ApplicationUser> userManager)
    {
        _userManager = userManager;
    }

    public async Task<Result<UserResponse>> GetByIdAsync(Guid userId)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user is null)
        {
            return Result.Failure<UserResponse>("User not found.", "USER_NOT_FOUND");
        }

        return Result.Success(new UserResponse(user.Id, user.FirstName, user.LastName, user.Email ?? string.Empty, user.CreatedAt));
    }
}
