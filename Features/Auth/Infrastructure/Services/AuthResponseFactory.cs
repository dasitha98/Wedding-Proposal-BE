using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Wedding_Proposal_BE.Features.Auth.Application.DTOs;
using Wedding_Proposal_BE.Features.Auth.Application.Interfaces;
using Wedding_Proposal_BE.Features.Auth.Domain.Entities;
using Wedding_Proposal_BE.Features.Auth.Infrastructure.Options;
using Wedding_Proposal_BE.Shared.Infrastructure.Data;
using Wedding_Proposal_BE.Shared.Infrastructure.Identity;

namespace Wedding_Proposal_BE.Features.Auth.Infrastructure.Services;

public class AuthResponseFactory : IAuthResponseFactory
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ITokenService _tokenService;
    private readonly AppDbContext _dbContext;
    private readonly JwtOptions _jwtOptions;

    public AuthResponseFactory(
        UserManager<ApplicationUser> userManager,
        ITokenService tokenService,
        AppDbContext dbContext,
        IOptions<JwtOptions> jwtOptions)
    {
        _userManager = userManager;
        _tokenService = tokenService;
        _dbContext = dbContext;
        _jwtOptions = jwtOptions.Value;
    }

    public async Task<AuthResponse> IssueAsync(ApplicationUser user)
    {
        var roles = await _userManager.GetRolesAsync(user);
        var (accessToken, expiresAt) = _tokenService.CreateAccessToken(user, roles);

        var refreshTokenValue = _tokenService.GenerateRefreshTokenValue();
        var refreshToken = RefreshToken.Issue(user.Id, refreshTokenValue, TimeSpan.FromDays(_jwtOptions.RefreshTokenDays));

        _dbContext.RefreshTokens.Add(refreshToken);
        await _dbContext.SaveChangesAsync();

        return new AuthResponse(accessToken, expiresAt, refreshTokenValue, ToUserResponse(user, roles));
    }

    public async Task<AuthUserResponse> ToUserResponseAsync(ApplicationUser user)
    {
        var roles = await _userManager.GetRolesAsync(user);
        return ToUserResponse(user, roles);
    }

    private static AuthUserResponse ToUserResponse(ApplicationUser user, IList<string> roles) =>
        new(user.Id, user.FirstName, user.LastName, user.Email ?? string.Empty, user.EmailConfirmed, roles.ToList());
}
