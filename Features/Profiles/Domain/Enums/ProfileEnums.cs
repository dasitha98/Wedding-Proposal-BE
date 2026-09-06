using System.Text.Json.Serialization;

namespace Wedding_Proposal_BE.Features.Profiles.Domain.Enums;

public enum Gender
{
    Male,
    Female
}

public enum MaritalStatus
{
    NeverMarried,
    Divorced,
    Widowed,
    Separated,
    AwaitingDivorce,
    LimitedToSignature
}

public enum ManagedBy
{
    Self,
    Parent,
    Guardian
}

public enum EducationStatus
{
    Ol,
    Al,
    Diploma,
    Bachelors,
    Masters,
    Doctorate,
    Professional,
    Other
}

public enum QualificationStatus
{
    CurrentlyFollowing,
    Completed
}

public enum JobStatus
{
    Employed,
    SelfEmployed,
    BusinessOwner,
    Unemployed,
    Student,
    Other
}

/// Member names diverge from the FE's numeric-leading literals ("25kTo50k" isn't a valid C#
/// identifier), so JsonStringEnumMemberName pins the exact wire value per member instead of
/// relying on the camelCase naming policy configured in Program.cs.
public enum IncomeRange
{
    Below25k,
    [JsonStringEnumMemberName("25kTo50k")]
    From25kTo50k,
    [JsonStringEnumMemberName("50kTo100k")]
    From50kTo100k,
    [JsonStringEnumMemberName("100kTo250k")]
    From100kTo250k,
    [JsonStringEnumMemberName("250kTo500k")]
    From250kTo500k,
    [JsonStringEnumMemberName("500kTo1m")]
    From500kTo1m,
    Above1m,
    PreferNotToSay
}

public enum AssetsStatus
{
    NotAvailable,
    Available,
    PreferNotToSay
}

public enum LifestyleHabit
{
    Yes,
    No,
    Occasionally,
    PreferNotToSay
}

public enum SiblingRelationship
{
    OlderBrother,
    YoungerBrother,
    OlderSister,
    YoungerSister
}

/// Viewer-relative connection state between two users (mirrors the FE's RelationshipStatus:
/// none/sent/pending/accepted/declined/connected). Not stored directly — derived by
/// ProfileService from the requester/target rows below, relative to the viewing user.
public enum ConnectionStatus
{
    Sent,
    Accepted,
    Declined
}
