using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Wedding_Proposal_BE.Features.Auth.Application.DTOs;
using Wedding_Proposal_BE.Features.Auth.Application.Interfaces;
using Wedding_Proposal_BE.Features.Auth.Domain.Entities;
using Wedding_Proposal_BE.Features.Auth.Infrastructure.Options;
using Wedding_Proposal_BE.Shared.Common;
using Wedding_Proposal_BE.Shared.Infrastructure.Data;
using Wedding_Proposal_BE.Shared.Infrastructure.Identity;
using Microsoft.Extensions.Options;

namespace Wedding_Proposal_BE.Features.Auth.Infrastructure.Services;

public class SessionService : ISessionService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ITokenService _tokenService;
    private readonly AppDbContext _dbContext;
    private readonly JwtOptions _jwtOptions;
    private readonly IAuthResponseFactory _authResponseFactory;

    public SessionService(
        UserManager<ApplicationUser> userManager,
        ITokenService tokenService,
        AppDbContext dbContext,
        IOptions<JwtOptions> jwtOptions,
        IAuthResponseFactory authResponseFactory)
    {
        _userManager = userManager;
        _tokenService = tokenService;
        _dbContext = dbContext;
        _jwtOptions = jwtOptions.Value;
        _authResponseFactory = authResponseFactory;
    }

    public async Task<Result<AuthResponse>> LoginAsync(LoginRequest request)
    {
        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user is null)
        {
            return Result.Failure<AuthResponse>("Invalid email or password.", "INVALID_CREDENTIALS");
        }

        if (await _userManager.IsLockedOutAsync(user))
        {
            return Result.Failure<AuthResponse>("This account has been suspended. Contact support for help.", "ACCOUNT_LOCKED");
        }

        var passwordValid = await _userManager.CheckPasswordAsync(user, request.Password);
        if (!passwordValid)
        {
            return Result.Failure<AuthResponse>("Invalid email or password.", "INVALID_CREDENTIALS");
        }

        if (!user.EmailConfirmed)
        {
            return Result.Failure<AuthResponse>("Please verify your email before signing in.", "EMAIL_NOT_VERIFIED");
        }

        return Result.Success(await _authResponseFactory.IssueAsync(user));
    }

    public async Task<Result<AuthResponse>> RefreshAsync(string refreshToken)
    {
        var existingToken = await _dbContext.RefreshTokens
            .FirstOrDefaultAsync(rt => rt.Token == refreshToken);

        if (existingToken is null || !existingToken.IsActive)
        {
            return Result.Failure<AuthResponse>("Refresh token is invalid or expired.", "INVALID_REFRESH_TOKEN");
        }

        var user = await _userManager.FindByIdAsync(existingToken.UserId.ToString());
        if (user is null)
        {
            return Result.Failure<AuthResponse>("Refresh token is invalid or expired.", "INVALID_REFRESH_TOKEN");
        }

        var newRefreshTokenValue = _tokenService.GenerateRefreshTokenValue();
        existingToken.Revoke(newRefreshTokenValue);

        var newRefreshToken = RefreshToken.Issue(user.Id, newRefreshTokenValue, TimeSpan.FromDays(_jwtOptions.RefreshTokenDays));
        _dbContext.RefreshTokens.Add(newRefreshToken);
        await _dbContext.SaveChangesAsync();

        var roles = await _userManager.GetRolesAsync(user);
        var (accessToken, expiresAt) = _tokenService.CreateAccessToken(user, roles);

        return Result.Success(new AuthResponse(accessToken, expiresAt, newRefreshTokenValue, await _authResponseFactory.ToUserResponseAsync(user)));
    }

    public async Task<Result> RevokeAsync(string refreshToken)
    {
        var existingToken = await _dbContext.RefreshTokens
            .FirstOrDefaultAsync(rt => rt.Token == refreshToken);

        if (existingToken is null || !existingToken.IsActive)
        {
            return Result.Failure("Refresh token is invalid or already revoked.", "INVALID_REFRESH_TOKEN");
        }

        existingToken.Revoke();
        await _dbContext.SaveChangesAsync();

        return Result.Success();
    }
}
