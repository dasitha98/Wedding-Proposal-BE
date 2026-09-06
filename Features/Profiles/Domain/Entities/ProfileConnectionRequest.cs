using Wedding_Proposal_BE.Features.Profiles.Domain.Enums;

namespace Wedding_Proposal_BE.Features.Profiles.Domain.Entities;

/// One row per (requester, target) pair, keyed by ApplicationUser.Id on both sides (not
/// Profile.Id) since a connection is between accounts. The FE's viewer-relative
/// RelationshipStatus ("sent" for the requester vs "pending" for the target on the same row)
/// is derived by ProfileService by comparing this row's RequesterUserId to the viewer.
public class ProfileConnectionRequest
{
    /// Once a target has declined the same requester this many times, the requester is
    /// blocked from sending that target another request — see ProfileService.SendConnectionRequestAsync.
    public const int MaxDeclinesBeforeBlocked = 2;

    public Guid Id { get; private set; }
    public Guid RequesterUserId { get; private set; }
    public Guid TargetUserId { get; private set; }
    public ConnectionStatus Status { get; private set; }
    public int DeclineCount { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    public bool IsBlocked => DeclineCount >= MaxDeclinesBeforeBlocked;

    private ProfileConnectionRequest()
    {
    }

    public static ProfileConnectionRequest Create(Guid requesterUserId, Guid targetUserId)
    {
        var now = DateTime.UtcNow;
        return new ProfileConnectionRequest
        {
            Id = Guid.NewGuid(),
            RequesterUserId = requesterUserId,
            TargetUserId = targetUserId,
            Status = ConnectionStatus.Sent,
            DeclineCount = 0,
            CreatedAt = now,
            UpdatedAt = now
        };
    }

    public void Accept()
    {
        Status = ConnectionStatus.Accepted;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Decline()
    {
        Status = ConnectionStatus.Declined;
        DeclineCount++;
        UpdatedAt = DateTime.UtcNow;
    }

    /// Re-sends a previously declined request as a fresh "Sent" one, reusing the same row (a
    /// unique index on RequesterUserId/TargetUserId allows only one row per pair) so
    /// DeclineCount carries over across resends. Callers must check IsBlocked first.
    public void Resend()
    {
        Status = ConnectionStatus.Sent;
        UpdatedAt = DateTime.UtcNow;
    }
}
