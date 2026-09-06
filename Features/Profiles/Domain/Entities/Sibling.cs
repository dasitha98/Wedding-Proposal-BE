using Wedding_Proposal_BE.Features.Profiles.Domain.Enums;

namespace Wedding_Proposal_BE.Features.Profiles.Domain.Entities;

public class Sibling
{
    public Guid Id { get; private set; }
    public Guid ProfileId { get; private set; }
    public SiblingRelationship Relationship { get; private set; }
    public MaritalStatus MaritalStatus { get; private set; }
    public string? Occupation { get; private set; }

    private Sibling()
    {
    }

    public static Sibling Create(Guid profileId, SiblingRelationship relationship, MaritalStatus maritalStatus, string? occupation) =>
        new()
        {
            Id = Guid.NewGuid(),
            ProfileId = profileId,
            Relationship = relationship,
            MaritalStatus = maritalStatus,
            Occupation = occupation
        };
}
