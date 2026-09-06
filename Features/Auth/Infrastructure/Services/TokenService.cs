using System.Security.Claims;
using System.Security.Cryptography;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Wedding_Proposal_BE.Features.Auth.Application.Interfaces;
using Wedding_Proposal_BE.Features.Auth.Infrastructure.Options;
using Wedding_Proposal_BE.Shared.Infrastructure.Identity;

namespace Wedding_Proposal_BE.Features.Auth.Infrastructure.Services;

/// Single responsibility: knows only how to create/validate JWTs. Does not touch the
/// database, password hashing, or the register/login use case (that's AuthService).
public class TokenService : ITokenService
{
    private readonly JwtOptions _options;
    private readonly TokenClaimsFactory _claimsFactory;

    public TokenService(IOptions<JwtOptions> options, TokenClaimsFactory claimsFactory)
    {
        _options = options.Value;
        _claimsFactory = claimsFactory;
    }

    public (string Token, DateTime ExpiresAt) CreateAccessToken(ApplicationUser user, IEnumerable<string> roles)
    {
        var claims = _claimsFactory.CreateClaims(user, roles);
        var expiresAt = DateTime.UtcNow.AddMinutes(_options.AccessTokenMinutes);

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_options.SigningKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _options.Issuer,
            audience: _options.Audience,
            claims: claims,
            expires: expiresAt,
            signingCredentials: credentials);

        return (new JwtSecurityTokenHandler().WriteToken(token), expiresAt);
    }

    public string GenerateRefreshTokenValue()
    {
        var bytes = RandomNumberGenerator.GetBytes(64);
        return Convert.ToBase64String(bytes);
    }

    public Guid? ValidateAccessTokenAndGetUserId(string accessToken)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_options.SigningKey));
        var handler = new JwtSecurityTokenHandler();

        try
        {
            var principal = handler.ValidateToken(accessToken, new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidIssuer = _options.Issuer,
                ValidateAudience = true,
                ValidAudience = _options.Audience,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = key,
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero
            }, out _);

            var subject = principal.FindFirstValue(ClaimTypes.NameIdentifier);
            return Guid.TryParse(subject, out var userId) ? userId : null;
        }
        catch (SecurityTokenException)
        {
            return null;
        }
    }

    public string CreateResetToken(string email, TimeSpan lifetime)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.Email, email),
            new("type", "reset")
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_options.SigningKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _options.Issuer,
            audience: _options.Audience,
            claims: claims,
            expires: DateTime.UtcNow.Add(lifetime),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public string? ValidateResetTokenAndGetEmail(string resetToken)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_options.SigningKey));
        var handler = new JwtSecurityTokenHandler();

        try
        {
            var principal = handler.ValidateToken(resetToken, new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidIssuer = _options.Issuer,
                ValidateAudience = true,
                ValidAudience = _options.Audience,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = key,
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero
            }, out _);

            if (principal.FindFirstValue("type") != "reset")
            {
                return null;
            }

            return principal.FindFirstValue(ClaimTypes.Email);
        }
        catch (SecurityTokenException)
        {
            return null;
        }
    }
}
