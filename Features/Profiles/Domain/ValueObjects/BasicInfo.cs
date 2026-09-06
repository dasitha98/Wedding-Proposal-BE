using Wedding_Proposal_BE.Features.Profiles.Domain.Enums;

namespace Wedding_Proposal_BE.Features.Profiles.Domain.ValueObjects;

/// EF owned type (no identity of its own) — always present on a Profile, mutated only as a
/// whole via Profile.UpdateBasicInfo.
public class BasicInfo
{
    public Gender Gender { get; private set; }
    public string Race { get; private set; } = string.Empty;
    public string Religion { get; private set; } = string.Empty;
    public string Caste { get; private set; } = string.Empty;
    public MaritalStatus MaritalStatus { get; private set; }
    public string HeightLabel { get; private set; } = string.Empty;

    private BasicInfo()
    {
    }

    public static BasicInfo CreateDefault() => new()
    {
        Gender = Gender.Male,
        Race = string.Empty,
        Religion = string.Empty,
        Caste = string.Empty,
        MaritalStatus = MaritalStatus.NeverMarried,
        HeightLabel = string.Empty
    };

    public static BasicInfo Create(Gender gender, string race, string religion, string caste, MaritalStatus maritalStatus, string heightLabel) =>
        new()
        {
            Gender = gender,
            Race = race,
            Religion = religion,
            Caste = caste,
            MaritalStatus = maritalStatus,
            HeightLabel = heightLabel
        };
}
