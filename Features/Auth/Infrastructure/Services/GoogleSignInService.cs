using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Wedding_Proposal_BE.Features.Auth.Application.DTOs;
using Wedding_Proposal_BE.Features.Auth.Application.Interfaces;
using Wedding_Proposal_BE.Features.Auth.Infrastructure.Options;
using Wedding_Proposal_BE.Shared.Common;
using Wedding_Proposal_BE.Shared.Infrastructure.Data;
using Wedding_Proposal_BE.Shared.Infrastructure.Identity;

namespace Wedding_Proposal_BE.Features.Auth.Infrastructure.Services;

public class GoogleSignInService : IGoogleSignInService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly AppDbContext _dbContext;
    private readonly GoogleAuthOptions _googleOptions;
    private readonly IGoogleOidcClient _googleOidcClient;
    private readonly IGoogleOidcTransactionStore _googleOidcTransactionStore;
    private readonly IAuthResponseFactory _authResponseFactory;

    public GoogleSignInService(
        UserManager<ApplicationUser> userManager,
        AppDbContext dbContext,
        IOptions<GoogleAuthOptions> googleOptions,
        IGoogleOidcClient googleOidcClient,
        IGoogleOidcTransactionStore googleOidcTransactionStore,
        IAuthResponseFactory authResponseFactory)
    {
        _userManager = userManager;
        _dbContext = dbContext;
        _googleOptions = googleOptions.Value;
        _googleOidcClient = googleOidcClient;
        _googleOidcTransactionStore = googleOidcTransactionStore;
        _authResponseFactory = authResponseFactory;
    }

    public Task<Result<GoogleAuthStartResponse>> StartGoogleSignInAsync(string redirectUri)
    {
        if (!IsAllowedRedirectUri(redirectUri))
        {
            return Task.FromResult(Result.Failure<GoogleAuthStartResponse>("This redirect URI is not allowed.", "INVALID_REDIRECT_URI"));
        }

        var codeVerifier = GeneratePkceCodeVerifier();
        var codeChallenge = ComputePkceCodeChallenge(codeVerifier);
        var nonce = GenerateUrlSafeRandomValue();
        var state = _googleOidcTransactionStore.Create(codeVerifier, nonce, redirectUri);

        var authorizationUrl = BuildGoogleAuthorizationUrl(redirectUri, state, nonce, codeChallenge);

        return Task.FromResult(Result.Success(new GoogleAuthStartResponse(authorizationUrl, state)));
    }

    public async Task<Result<AuthResponse>> CompleteGoogleSignInAsync(string code, string state)
    {
        if (!_googleOidcTransactionStore.TryConsume(state, out var transaction))
        {
            return Result.Failure<AuthResponse>("This sign-in attempt has expired. Please try again.", "INVALID_STATE");
        }

        GoogleIdentity identity;
        try
        {
            identity = await _googleOidcClient.AuthenticateAsync(
                code, transaction.CodeVerifier, transaction.RedirectUri, transaction.Nonce, CancellationToken.None);
        }
        catch (GoogleOidcException)
        {
            return Result.Failure<AuthResponse>("Google sign-in failed.", "INVALID_GOOGLE_TOKEN");
        }

        var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.GoogleSubject == identity.Subject);
        if (user is not null)
        {
            user.UpdateGoogleProfile(identity.Name, identity.PictureUrl);
            await _dbContext.SaveChangesAsync();
            return Result.Success(await _authResponseFactory.IssueAsync(user));
        }

        var existingByEmail = await _userManager.FindByEmailAsync(identity.Email);
        if (existingByEmail is not null)
        {
            // Only link into an existing password-registered account once Google itself has
            // confirmed the user owns this email — otherwise anyone could take over an account
            // by signing up on Google with someone else's (unverified) email address.
            if (!identity.EmailVerified)
            {
                return Result.Failure<AuthResponse>(
                    "An account with this email already exists. Sign in with your password instead.", "EMAIL_IN_USE");
            }

            existingByEmail.LinkGoogleAccount(identity.Subject, identity.Name, identity.PictureUrl);
            if (!existingByEmail.EmailConfirmed)
            {
                existingByEmail.EmailConfirmed = true;
            }
            await _userManager.UpdateAsync(existingByEmail);
            return Result.Success(await _authResponseFactory.IssueAsync(existingByEmail));
        }

        var newUser = ApplicationUser.CreateFromGoogle(
            identity.Subject, identity.Email, identity.EmailVerified, identity.Name,
            firstName: null, lastName: null, identity.PictureUrl);

        var createResult = await _userManager.CreateAsync(newUser);
        if (!createResult.Succeeded)
        {
            var message = string.Join(" ", createResult.Errors.Select(e => e.Description));
            return Result.Failure<AuthResponse>(message, "REGISTRATION_FAILED");
        }

        await _userManager.AddToRoleAsync(newUser, RoleNames.User);

        return Result.Success(await _authResponseFactory.IssueAsync(newUser));
    }

    private bool IsAllowedRedirectUri(string redirectUri)
    {
        foreach (var allowed in _googleOptions.AllowedRedirectUris)
        {
            if (allowed.EndsWith("://", StringComparison.Ordinal))
            {
                if (redirectUri.StartsWith(allowed, StringComparison.Ordinal)) return true;
            }
            else if (string.Equals(redirectUri, allowed, StringComparison.Ordinal))
            {
                return true;
            }
        }

        return false;
    }

    /// Builds the request to Google's OIDC authorization endpoint per
    /// https://developers.google.com/identity/openid-connect/openid-connect#sendauthrequest.
    /// `response_type=code` + `code_challenge`/`S256` requests Authorization Code + PKCE;
    /// `nonce` is echoed back inside the ID token and checked in GoogleOidcClient.
    private string BuildGoogleAuthorizationUrl(string redirectUri, string state, string nonce, string codeChallenge)
    {
        var query = new Dictionary<string, string>
        {
            ["client_id"] = _googleOptions.ClientId,
            ["redirect_uri"] = redirectUri,
            ["response_type"] = "code",
            ["scope"] = "openid email profile",
            ["state"] = state,
            ["nonce"] = nonce,
            ["code_challenge"] = codeChallenge,
            ["code_challenge_method"] = "S256",
            ["access_type"] = "online",
            ["prompt"] = "select_account",
        };

        var queryString = string.Join('&', query.Select(kv => $"{Uri.EscapeDataString(kv.Key)}={Uri.EscapeDataString(kv.Value)}"));
        return $"https://accounts.google.com/o/oauth2/v2/auth?{queryString}";
    }

    private static string GeneratePkceCodeVerifier() => GenerateUrlSafeRandomValue(bytesLength: 32);

    private static string ComputePkceCodeChallenge(string codeVerifier)
    {
        var hash = SHA256.HashData(Encoding.ASCII.GetBytes(codeVerifier));
        return Convert.ToBase64String(hash).Replace('+', '-').Replace('/', '_').TrimEnd('=');
    }

    private static string GenerateUrlSafeRandomValue(int bytesLength = 32)
    {
        var bytes = RandomNumberGenerator.GetBytes(bytesLength);
        return Convert.ToBase64String(bytes).Replace('+', '-').Replace('/', '_').TrimEnd('=');
    }
}
