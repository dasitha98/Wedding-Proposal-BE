namespace Wedding_Proposal_BE.Features.Profiles.Domain.Entities;

/// One row per (viewer, favourited-profile) pair. UserId is the account that favourited
/// TargetProfileId (a Profile.Id, not a UserId).
public class ProfileFavourite
{
    public Guid Id { get; private set; }
    public Guid UserId { get; private set; }
    public Guid TargetProfileId { get; private set; }
    public DateTime CreatedAt { get; private set; }

    private ProfileFavourite()
    {
    }

    public static ProfileFavourite Create(Guid userId, Guid targetProfileId) =>
        new() { Id = Guid.NewGuid(), UserId = userId, TargetProfileId = targetProfileId, CreatedAt = DateTime.UtcNow };
}
