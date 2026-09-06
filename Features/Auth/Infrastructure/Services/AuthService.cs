using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Wedding_Proposal_BE.Features.Auth.Application.DTOs;
using Wedding_Proposal_BE.Features.Auth.Application.Interfaces;
using Wedding_Proposal_BE.Features.Auth.Domain.Entities;
using Wedding_Proposal_BE.Features.Auth.Infrastructure.Options;
using Wedding_Proposal_BE.Shared.Common;
using Wedding_Proposal_BE.Shared.Infrastructure.Data;
using Wedding_Proposal_BE.Shared.Infrastructure.Identity;

namespace Wedding_Proposal_BE.Features.Auth.Infrastructure.Services;

/// Orchestrates register/login/refresh/revoke and the account-management/password-reset/
/// Google-sign-in use cases. Depends only on abstractions (ITokenService, IOtpSender,
/// UserManager<T>) injected via constructor — never constructs its collaborators itself
/// (Dependency Inversion, Composition over inheritance).
public class AuthService : IAuthService
{
    private static readonly TimeSpan OtpValidFor = TimeSpan.FromMinutes(5);
    private static readonly TimeSpan OtpResendCooldown = TimeSpan.FromSeconds(60);
    private static readonly TimeSpan ResetTokenValidFor = TimeSpan.FromMinutes(10);

    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ITokenService _tokenService;
    private readonly IOtpSender _otpSender;
    private readonly AppDbContext _dbContext;
    private readonly JwtOptions _jwtOptions;
    private readonly GoogleAuthOptions _googleOptions;
    private readonly IGoogleOidcClient _googleOidcClient;
    private readonly IGoogleOidcTransactionStore _googleOidcTransactionStore;

    public AuthService(
        UserManager<ApplicationUser> userManager,
        ITokenService tokenService,
        IOtpSender otpSender,
        AppDbContext dbContext,
        IOptions<JwtOptions> jwtOptions,
        IOptions<GoogleAuthOptions> googleOptions,
        IGoogleOidcClient googleOidcClient,
        IGoogleOidcTransactionStore googleOidcTransactionStore)
    {
        _userManager = userManager;
        _tokenService = tokenService;
        _otpSender = otpSender;
        _dbContext = dbContext;
        _jwtOptions = jwtOptions.Value;
        _googleOptions = googleOptions.Value;
        _googleOidcClient = googleOidcClient;
        _googleOidcTransactionStore = googleOidcTransactionStore;
    }

    public async Task<Result<RegisterResponse>> RegisterAsync(RegisterRequest request)
    {
        var normalizedEmail = request.Email.Trim().ToLowerInvariant();
        var existing = await _userManager.FindByEmailAsync(normalizedEmail);
        if (existing is not null)
        {
            if (existing.EmailConfirmed)
            {
                return Result.Failure<RegisterResponse>("A user with this email already exists.", "EMAIL_IN_USE");
            }

            // Unverified account from an abandoned signup attempt — treat this as a retry
            // rather than a dead end: refresh the name/password to whatever was just
            // submitted (in case the first attempt had a typo) and re-issue an OTP.
            existing.UpdateName(request.FirstName, request.LastName);
            await _userManager.UpdateAsync(existing);
            var passwordResetToken = await _userManager.GeneratePasswordResetTokenAsync(existing);
            await _userManager.ResetPasswordAsync(existing, passwordResetToken, request.Password);

            return await IssueEmailVerificationOtpAsync(normalizedEmail);
        }

        var user = ApplicationUser.Create(normalizedEmail, request.FirstName, request.LastName);
        var createResult = await _userManager.CreateAsync(user, request.Password);
        if (!createResult.Succeeded)
        {
            var message = string.Join(" ", createResult.Errors.Select(e => e.Description));
            return Result.Failure<RegisterResponse>(message, "REGISTRATION_FAILED");
        }

        await _userManager.AddToRoleAsync(user, RoleNames.User);

        return await IssueEmailVerificationOtpAsync(normalizedEmail);
    }

    public async Task<Result<AuthResponse>> VerifyRegistrationOtpAsync(string email, string code)
    {
        var normalizedEmail = email.Trim().ToLowerInvariant();
        var otp = await _dbContext.EmailVerificationOtps.FirstOrDefaultAsync(o => o.Email == normalizedEmail);
        if (otp is null || otp.IsExpired || otp.IsConsumed)
        {
            return Result.Failure<AuthResponse>("This code has expired. Request a new one.", "OTP_EXPIRED");
        }

        if (!otp.TryConsume(HashOtp(code)))
        {
            await _dbContext.SaveChangesAsync();
            return otp.AttemptsRemaining <= 0
                ? Result.Failure<AuthResponse>("Too many incorrect attempts. Request a new code.", "OTP_LOCKED")
                : Result.Failure<AuthResponse>($"Incorrect code. {otp.AttemptsRemaining} attempt(s) remaining.", "OTP_INCORRECT");
        }

        var user = await _userManager.FindByEmailAsync(normalizedEmail);
        if (user is null)
        {
            return Result.Failure<AuthResponse>("Account not found.", "USER_NOT_FOUND");
        }

        if (!user.EmailConfirmed)
        {
            user.EmailConfirmed = true;
            await _userManager.UpdateAsync(user);
        }

        await _dbContext.SaveChangesAsync();

        return await IssueAuthResponseAsync(user);
    }

    public async Task<Result<RegisterResponse>> ResendRegistrationOtpAsync(string email)
    {
        var normalizedEmail = email.Trim().ToLowerInvariant();
        var user = await _userManager.FindByEmailAsync(normalizedEmail);
        if (user is null)
        {
            return Result.Failure<RegisterResponse>("No pending registration found for that email.", "USER_NOT_FOUND");
        }

        if (user.EmailConfirmed)
        {
            return Result.Failure<RegisterResponse>("This account is already verified.", "ALREADY_VERIFIED");
        }

        return await IssueEmailVerificationOtpAsync(normalizedEmail);
    }

    private async Task<Result<RegisterResponse>> IssueEmailVerificationOtpAsync(string normalizedEmail)
    {
        var existing = await _dbContext.EmailVerificationOtps.FirstOrDefaultAsync(o => o.Email == normalizedEmail);
        if (existing is not null)
        {
            if (existing.ResendAvailableAt > DateTime.UtcNow)
            {
                var waitSeconds = (int)Math.Ceiling((existing.ResendAvailableAt - DateTime.UtcNow).TotalSeconds);
                return Result.Failure<RegisterResponse>($"Please wait {waitSeconds}s before requesting another code.", "OTP_COOLDOWN");
            }

            _dbContext.EmailVerificationOtps.Remove(existing);
        }

        var code = GenerateOtpCode();
        var otp = EmailVerificationOtp.Issue(normalizedEmail, HashOtp(code), OtpValidFor, OtpResendCooldown);
        _dbContext.EmailVerificationOtps.Add(otp);
        await _dbContext.SaveChangesAsync();

        await _otpSender.SendAsync(normalizedEmail, code);

        return Result.Success(new RegisterResponse(normalizedEmail, (int)OtpValidFor.TotalSeconds, (int)OtpResendCooldown.TotalSeconds));
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

        return await IssueAuthResponseAsync(user);
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

        return Result.Success(new AuthResponse(accessToken, expiresAt, newRefreshTokenValue, ToUserResponse(user, roles)));
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

    public async Task<Result<RequestPasswordResetOtpResponse>> RequestPasswordResetOtpAsync(string email)
    {
        var normalizedEmail = email.Trim().ToLowerInvariant();
        var user = await _userManager.FindByEmailAsync(normalizedEmail);
        if (user is null)
        {
            return Result.Failure<RequestPasswordResetOtpResponse>("No account found with that email address.", "USER_NOT_FOUND");
        }

        var existing = await _dbContext.PasswordResetOtps.FirstOrDefaultAsync(o => o.Email == normalizedEmail);
        if (existing is not null)
        {
            if (existing.ResendAvailableAt > DateTime.UtcNow)
            {
                var waitSeconds = (int)Math.Ceiling((existing.ResendAvailableAt - DateTime.UtcNow).TotalSeconds);
                return Result.Failure<RequestPasswordResetOtpResponse>($"Please wait {waitSeconds}s before requesting another code.", "OTP_COOLDOWN");
            }

            _dbContext.PasswordResetOtps.Remove(existing);
        }

        var code = GenerateOtpCode();
        var otp = PasswordResetOtp.Issue(normalizedEmail, HashOtp(code), OtpValidFor, OtpResendCooldown);
        _dbContext.PasswordResetOtps.Add(otp);
        await _dbContext.SaveChangesAsync();

        await _otpSender.SendAsync(normalizedEmail, code);

        return Result.Success(new RequestPasswordResetOtpResponse((int)OtpValidFor.TotalSeconds, (int)OtpResendCooldown.TotalSeconds));
    }

    public async Task<Result<VerifyPasswordResetOtpResponse>> VerifyPasswordResetOtpAsync(string email, string code)
    {
        var normalizedEmail = email.Trim().ToLowerInvariant();
        var otp = await _dbContext.PasswordResetOtps.FirstOrDefaultAsync(o => o.Email == normalizedEmail);
        if (otp is null || otp.IsExpired || otp.IsConsumed)
        {
            return Result.Failure<VerifyPasswordResetOtpResponse>("This code has expired. Request a new one.", "OTP_EXPIRED");
        }

        if (!otp.TryConsume(HashOtp(code)))
        {
            await _dbContext.SaveChangesAsync();
            return otp.AttemptsRemaining <= 0
                ? Result.Failure<VerifyPasswordResetOtpResponse>("Too many incorrect attempts. Request a new code.", "OTP_LOCKED")
                : Result.Failure<VerifyPasswordResetOtpResponse>($"Incorrect code. {otp.AttemptsRemaining} attempt(s) remaining.", "OTP_INCORRECT");
        }

        await _dbContext.SaveChangesAsync();

        var resetToken = _tokenService.CreateResetToken(normalizedEmail, ResetTokenValidFor);
        return Result.Success(new VerifyPasswordResetOtpResponse(resetToken));
    }

    public async Task<Result> ResetPasswordAsync(string resetToken, string newPassword)
    {
        var email = _tokenService.ValidateResetTokenAndGetEmail(resetToken);
        if (email is null)
        {
            return Result.Failure("This reset link has expired. Start over.", "INVALID_RESET_TOKEN");
        }

        var user = await _userManager.FindByEmailAsync(email);
        if (user is null)
        {
            return Result.Failure("This reset link has expired. Start over.", "INVALID_RESET_TOKEN");
        }

        var resetPasswordToken = await _userManager.GeneratePasswordResetTokenAsync(user);
        var result = await _userManager.ResetPasswordAsync(user, resetPasswordToken, newPassword);
        if (!result.Succeeded)
        {
            var message = string.Join(" ", result.Errors.Select(e => e.Description));
            return Result.Failure(message, "RESET_FAILED");
        }

        return Result.Success();
    }

    public async Task<Result> ChangePasswordAsync(Guid userId, string currentPassword, string newPassword)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user is null)
        {
            return Result.Failure("Account not found.", "USER_NOT_FOUND");
        }

        var result = await _userManager.ChangePasswordAsync(user, currentPassword, newPassword);
        if (!result.Succeeded)
        {
            var message = string.Join(" ", result.Errors.Select(e => e.Description));
            return Result.Failure(message, "CHANGE_PASSWORD_FAILED");
        }

        return Result.Success();
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

        var roles = await _userManager.GetRolesAsync(user);
        return Result.Success(ToUserResponse(user, roles));
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
            return await IssueAuthResponseAsync(user);
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
            return await IssueAuthResponseAsync(existingByEmail);
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

        return await IssueAuthResponseAsync(newUser);
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

    private async Task<Result<AuthResponse>> IssueAuthResponseAsync(ApplicationUser user)
    {
        var roles = await _userManager.GetRolesAsync(user);
        var (accessToken, expiresAt) = _tokenService.CreateAccessToken(user, roles);

        var refreshTokenValue = _tokenService.GenerateRefreshTokenValue();
        var refreshToken = RefreshToken.Issue(user.Id, refreshTokenValue, TimeSpan.FromDays(_jwtOptions.RefreshTokenDays));

        _dbContext.RefreshTokens.Add(refreshToken);
        await _dbContext.SaveChangesAsync();

        return Result.Success(new AuthResponse(accessToken, expiresAt, refreshTokenValue, ToUserResponse(user, roles)));
    }

    private static AuthUserResponse ToUserResponse(ApplicationUser user, IList<string> roles) =>
        new(user.Id, user.FirstName, user.LastName, user.Email ?? string.Empty, user.EmailConfirmed, roles.ToList());

    private static string GenerateOtpCode() => RandomNumberGenerator.GetInt32(100_000, 1_000_000).ToString();

    private static string HashOtp(string code)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(code));
        return Convert.ToHexString(bytes);
    }
}
