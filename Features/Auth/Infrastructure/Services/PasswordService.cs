using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Wedding_Proposal_BE.Features.Auth.Application.DTOs;
using Wedding_Proposal_BE.Features.Auth.Application.Interfaces;
using Wedding_Proposal_BE.Features.Auth.Domain.Entities;
using Wedding_Proposal_BE.Shared.Common;
using Wedding_Proposal_BE.Shared.Infrastructure.Data;
using Wedding_Proposal_BE.Shared.Infrastructure.Identity;

namespace Wedding_Proposal_BE.Features.Auth.Infrastructure.Services;

public class PasswordService : IPasswordService
{
    private static readonly TimeSpan OtpValidFor = TimeSpan.FromMinutes(5);
    private static readonly TimeSpan OtpResendCooldown = TimeSpan.FromSeconds(60);
    private static readonly TimeSpan ResetTokenValidFor = TimeSpan.FromMinutes(10);

    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IOtpSender _otpSender;
    private readonly AppDbContext _dbContext;
    private readonly ITokenService _tokenService;

    public PasswordService(
        UserManager<ApplicationUser> userManager,
        IOtpSender otpSender,
        AppDbContext dbContext,
        ITokenService tokenService)
    {
        _userManager = userManager;
        _otpSender = otpSender;
        _dbContext = dbContext;
        _tokenService = tokenService;
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

        var code = OtpCodeGenerator.GenerateCode();
        var otp = PasswordResetOtp.Issue(normalizedEmail, OtpCodeGenerator.Hash(code), OtpValidFor, OtpResendCooldown);
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

        if (!otp.TryConsume(OtpCodeGenerator.Hash(code)))
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
}
