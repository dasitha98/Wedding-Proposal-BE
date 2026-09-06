namespace Wedding_Proposal_BE.Features.Admin.Application.DTOs;

public record AdminRefreshTokenDto(
    Guid Id, Guid UserId, string UserEmail, DateTime ExpiresAt, DateTime CreatedAt, DateTime? RevokedAt, bool IsActive);

public record AdminOtpDto(
    Guid Id, string Email, DateTime ExpiresAt, DateTime ResendAvailableAt, int AttemptsRemaining, DateTime? ConsumedAt, string Type);
