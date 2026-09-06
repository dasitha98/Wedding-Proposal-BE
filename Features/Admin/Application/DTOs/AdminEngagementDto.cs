using Wedding_Proposal_BE.Features.Profiles.Domain.Enums;

namespace Wedding_Proposal_BE.Features.Admin.Application.DTOs;

public record AdminConnectionRequestDto(
    Guid Id, Guid RequesterUserId, string RequesterName, Guid TargetUserId, string TargetName,
    ConnectionStatus Status, int DeclineCount, DateTime CreatedAt, DateTime UpdatedAt);

public class AdminUpdateConnectionRequestStatusRequest
{
    public ConnectionStatus Status { get; set; }
}

public record AdminFavouriteDto(
    Guid Id, Guid UserId, string UserName, Guid TargetProfileId, string TargetProfileName, DateTime CreatedAt);

public record AdminConversationDto(
    Guid Id, Guid UserAId, string UserAName, Guid UserBId, string UserBName, DateTime CreatedAt, int MessageCount);

public record AdminMessageDto(
    Guid Id, Guid ConversationId, Guid SenderUserId, string SenderName, string Text, DateTime SentAt);
