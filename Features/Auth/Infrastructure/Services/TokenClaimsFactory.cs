using System.Security.Claims;
using Wedding_Proposal_BE.Shared.Infrastructure.Identity;

namespace Wedding_Proposal_BE.Features.Auth.Infrastructure.Services;

/// Factory Pattern: isolates claim-building so TokenService only knows how to turn claims
/// into a token, not how to derive claims from a user. New claim types are added here only.
public class TokenClaimsFactory
{
    public IReadOnlyCollection<Claim> CreateClaims(ApplicationUser user, IEnumerable<string> roles)
    {
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNamesCompat.Sub, user.Id.ToString()),
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Email, user.Email ?? string.Empty),
            new(JwtRegisteredClaimNamesCompat.Jti, Guid.NewGuid().ToString())
        };

        claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));

        return claims;
    }
}

/// Avoids a direct dependency on System.IdentityModel.Tokens.Jwt from this file.
internal static class JwtRegisteredClaimNamesCompat
{
    public const string Sub = "sub";
    public const string Jti = "jti";
}
