namespace Wedding_Proposal_BE.Features.Profiles.Domain.ValueObjects;

/// Sibling count/records live alongside this owned type on Profile (SiblingCount here,
/// the Sibling child collection separately) since FE's Family entity nests both.
public class Family
{
    public string? FatherOccupation { get; private set; }
    public string? MotherOccupation { get; private set; }
    public int SiblingCount { get; private set; }

    private Family()
    {
    }

    public static Family CreateDefault() => new() { SiblingCount = 0 };

    public static Family Create(string? fatherOccupation, string? motherOccupation, int siblingCount) =>
        new() { FatherOccupation = fatherOccupation, MotherOccupation = motherOccupation, SiblingCount = siblingCount };
}
