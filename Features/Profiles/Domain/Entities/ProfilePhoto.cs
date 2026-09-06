namespace Wedding_Proposal_BE.Features.Profiles.Domain.Entities;

public class ProfilePhoto
{
    public Guid Id { get; private set; }
    public Guid ProfileId { get; private set; }
    public string Uri { get; private set; } = string.Empty;
    public bool IsBlurred { get; private set; }
    public int Order { get; private set; }

    private ProfilePhoto()
    {
    }

    public static ProfilePhoto Create(Guid profileId, string uri, bool isBlurred, int order) =>
        new() { Id = Guid.NewGuid(), ProfileId = profileId, Uri = uri, IsBlurred = isBlurred, Order = order };
}
