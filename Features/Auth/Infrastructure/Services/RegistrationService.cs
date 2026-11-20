using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Wedding_Proposal_BE.Features.Auth.Application.DTOs;
using Wedding_Proposal_BE.Features.Auth.Application.Interfaces;
using Wedding_Proposal_BE.Features.Auth.Domain.Entities;
using Wedding_Proposal_BE.Shared.Common;
using Wedding_Proposal_BE.Shared.Infrastructure.Data;
using Wedding_Proposal_BE.Shared.Infrastructure.Identity;

namespace Wedding_Proposal_BE.Features.Auth.Infrastructure.Services;

public class RegistrationService : IRegistrationService
{
    private static readonly TimeSpan OtpValidFor = TimeSpan.FromMinutes(5);
    private static readonly TimeSpan OtpResendCooldown = TimeSpan.FromSeconds(60);

    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IOtpSender _otpSender;
    private readonly AppDbContext _dbContext;
    private readonly IAuthResponseFactory _authResponseFactory;

    public RegistrationService(
        UserManager<ApplicationUser> userManager,
        IOtpSender otpSender,
        AppDbContext dbContext,
        IAuthResponseFactory authResponseFactory)
    {
        _userManager = userManager;
        _otpSender = otpSender;
        _dbContext = dbContext;
        _authResponseFactory = authResponseFactory;
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

        if (!otp.TryConsume(OtpCodeGenerator.Hash(code)))
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

        return Result.Success(await _authResponseFactory.IssueAsync(user));
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

        var code = OtpCodeGenerator.GenerateCode();
        var otp = EmailVerificationOtp.Issue(normalizedEmail, OtpCodeGenerator.Hash(code), OtpValidFor, OtpResendCooldown);
        _dbContext.EmailVerificationOtps.Add(otp);
        await _dbContext.SaveChangesAsync();

        await _otpSender.SendAsync(normalizedEmail, code);

        return Result.Success(new RegisterResponse(normalizedEmail, (int)OtpValidFor.TotalSeconds, (int)OtpResendCooldown.TotalSeconds));
    }
}
