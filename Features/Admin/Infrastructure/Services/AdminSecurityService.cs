using Microsoft.EntityFrameworkCore;
using Wedding_Proposal_BE.Features.Admin.Application.DTOs;
using Wedding_Proposal_BE.Features.Admin.Application.Interfaces;
using Wedding_Proposal_BE.Shared.Common;
using Wedding_Proposal_BE.Shared.Infrastructure.Data;

namespace Wedding_Proposal_BE.Features.Admin.Infrastructure.Services;

public class AdminSecurityService : IAdminSecurityService
{
    private readonly AppDbContext _dbContext;

    public AdminSecurityService(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Result<AdminPagedResponse<AdminRefreshTokenDto>>> ListRefreshTokensAsync(int page, int pageSize)
    {
        var query = _dbContext.RefreshTokens.OrderByDescending(t => t.CreatedAt).AsQueryable();
        var paged = await AdminPaging.ToPagedAsync(query, page, pageSize, t => t);

        var userIds = paged.Items.Select(t => t.UserId).Distinct().ToList();
        var emails = await _dbContext.Users.Where(u => userIds.Contains(u.Id)).ToDictionaryAsync(u => u.Id, u => u.Email ?? string.Empty);

        var dtos = paged.Items.Select(t => new AdminRefreshTokenDto(
            t.Id, t.UserId, emails.GetValueOrDefault(t.UserId, "Unknown"), t.ExpiresAt, t.CreatedAt, t.RevokedAt, t.IsActive)).ToList();

        return Result.Success(new AdminPagedResponse<AdminRefreshTokenDto>(dtos, paged.TotalCount, paged.TotalPages, paged.Page, paged.PageSize));
    }

    public async Task<Result> DeleteRefreshTokenAsync(Guid id)
    {
        var token = await _dbContext.RefreshTokens.FirstOrDefaultAsync(t => t.Id == id);
        if (token is null)
        {
            return Result.Failure("Refresh token not found.", "TOKEN_NOT_FOUND");
        }

        _dbContext.RefreshTokens.Remove(token);
        await _dbContext.SaveChangesAsync();
        return Result.Success();
    }

    public async Task<Result<AdminPagedResponse<AdminOtpDto>>> ListPasswordResetOtpsAsync(int page, int pageSize)
    {
        var query = _dbContext.PasswordResetOtps.OrderByDescending(o => o.ExpiresAt).AsQueryable();
        var paged = await AdminPaging.ToPagedAsync(query, page, pageSize, o =>
            new AdminOtpDto(o.Id, o.Email, o.ExpiresAt, o.ResendAvailableAt, o.AttemptsRemaining, o.ConsumedAt, "PasswordReset"));

        return Result.Success(paged);
    }

    public async Task<Result> DeletePasswordResetOtpAsync(Guid id)
    {
        var otp = await _dbContext.PasswordResetOtps.FirstOrDefaultAsync(o => o.Id == id);
        if (otp is null)
        {
            return Result.Failure("OTP not found.", "OTP_NOT_FOUND");
        }

        _dbContext.PasswordResetOtps.Remove(otp);
        await _dbContext.SaveChangesAsync();
        return Result.Success();
    }

    public async Task<Result<AdminPagedResponse<AdminOtpDto>>> ListEmailVerificationOtpsAsync(int page, int pageSize)
    {
        var query = _dbContext.EmailVerificationOtps.OrderByDescending(o => o.ExpiresAt).AsQueryable();
        var paged = await AdminPaging.ToPagedAsync(query, page, pageSize, o =>
            new AdminOtpDto(o.Id, o.Email, o.ExpiresAt, o.ResendAvailableAt, o.AttemptsRemaining, o.ConsumedAt, "EmailVerification"));

        return Result.Success(paged);
    }

    public async Task<Result> DeleteEmailVerificationOtpAsync(Guid id)
    {
        var otp = await _dbContext.EmailVerificationOtps.FirstOrDefaultAsync(o => o.Id == id);
        if (otp is null)
        {
            return Result.Failure("OTP not found.", "OTP_NOT_FOUND");
        }

        _dbContext.EmailVerificationOtps.Remove(otp);
        await _dbContext.SaveChangesAsync();
        return Result.Success();
    }
}
