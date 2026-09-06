namespace Wedding_Proposal_BE.Features.Messages.Domain.Entities;

public class Message
{
    public Guid Id { get; private set; }
    public Guid ConversationId { get; private set; }
    public Guid SenderUserId { get; private set; }
    public string Text { get; private set; } = string.Empty;
    public DateTime SentAt { get; private set; }

    private Message()
    {
    }

    public static Message Create(Guid conversationId, Guid senderUserId, string text) => new()
    {
        Id = Guid.NewGuid(),
        ConversationId = conversationId,
        SenderUserId = senderUserId,
        Text = text,
        SentAt = DateTime.UtcNow
    };
}
