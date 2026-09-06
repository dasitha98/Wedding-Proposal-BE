using Wedding_Proposal_BE.Features.Profiles.Application.DTOs;

namespace Wedding_Proposal_BE.Features.Messages.Application.DTOs;

public record MessageResponse(Guid Id, string Text, Guid SenderUserId, DateTime SentAt);

public record ConversationSummaryResponse(
    Guid ProfileId,
    ProfileSummaryResponse OtherParty,
    string? LastMessageText,
    DateTime? LastMessageAt,
    bool IsLastMessageMine,
    int UnreadCount);

public class SendMessageRequest
{
    public string Text { get; set; } = string.Empty;
}
