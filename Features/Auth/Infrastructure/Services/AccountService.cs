using Microsoft.AspNetCore.Identity;
using Wedding_Proposal_BE.Features.Auth.Application.DTOs;
using Wedding_Proposal_BE.Features.Auth.Application.Interfaces;
using Wedding_Proposal_BE.Shared.Common;
using Wedding_Proposal_BE.Shared.Infrastructure.Identity;

namespace Wedding_Proposal_BE.Features.Auth.Infrastructure.Services;

public class AccountService : IAccountService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IAuthResponseFactory _authResponseFactory;

    public AccountService(UserManager<ApplicationUser> userManager, IAuthResponseFactory authResponseFactory)
    {
        _userManager = userManager;
        _authResponseFactory = authResponseFactory;
    }

    public async Task<Result<AuthUserResponse>> UpdateAccountAsync(Guid userId, UpdateAccountRequest request)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user is null)
        {
            return Result.Failure<AuthUserResponse>("Account not found.", "USER_NOT_FOUND");
        }

        var normalizedEmail = request.Email.Trim().ToLowerInvariant();
        if (!string.Equals(normalizedEmail, user.Email, StringComparison.OrdinalIgnoreCase))
        {
            var owner = await _userManager.FindByEmailAsync(normalizedEmail);
            if (owner is not null && owner.Id != user.Id)
            {
                return Result.Failure<AuthUserResponse>("An account with that email already exists.", "EMAIL_IN_USE");
            }

            await _userManager.SetEmailAsync(user, normalizedEmail);
            await _userManager.SetUserNameAsync(user, normalizedEmail);
        }

        user.UpdateName(request.FirstName.Trim(), request.LastName.Trim());
        await _userManager.UpdateAsync(user);

        return Result.Success(await _authResponseFactory.ToUserResponseAsync(user));
    }
}
