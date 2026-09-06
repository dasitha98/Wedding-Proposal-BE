namespace Wedding_Proposal_BE.Features.Messages.Domain.Entities;

/// One row per pair of connected users. UserAId is always the lexicographically/numerically
/// smaller Guid of the pair (see Create) so a unique index on (UserAId, UserBId) dedupes
/// regardless of who messages first.
public class Conversation
{
    public Guid Id { get; private set; }
    public Guid UserAId { get; private set; }
    public Guid UserBId { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? LastReadAtA { get; private set; }
    public DateTime? LastReadAtB { get; private set; }

    private Conversation()
    {
    }

    public static Conversation Create(Guid userId1, Guid userId2)
    {
        var (a, b) = userId1.CompareTo(userId2) <= 0 ? (userId1, userId2) : (userId2, userId1);
        return new Conversation { Id = Guid.NewGuid(), UserAId = a, UserBId = b, CreatedAt = DateTime.UtcNow };
    }

    public void MarkReadFor(Guid userId)
    {
        var now = DateTime.UtcNow;
        if (userId == UserAId) LastReadAtA = now;
        else if (userId == UserBId) LastReadAtB = now;
    }

    public DateTime? LastReadAtFor(Guid userId) => userId == UserAId ? LastReadAtA : LastReadAtB;
}
