using Wedding_Proposal_BE.Features.Profiles.Domain.Enums;

namespace Wedding_Proposal_BE.Features.Profiles.Application.DTOs;

public record PhotoResponse(string Uri, bool IsBlurred);

/// Lightweight profile shape for list screens (requests, favourites) that don't need the
/// full ProfileResponse's sectioned detail.
public record ProfileSummaryResponse(
    Guid Id,
    string FirstName,
    string LastName,
    string? Photo,
    int Age,
    string City,
    string LastActiveLabel,
    bool IsGold);

public record ConnectionRequestResponse(
    Guid RequestId,
    ProfileSummaryResponse OtherParty,
    string Status,
    DateTime CreatedAt);

public record BasicInfoResponse(Gender Gender, string Race, string Religion, string Caste, string DateOfBirthMasked, MaritalStatus MaritalStatus, string HeightLabel);

public record EducationResponse(EducationStatus Qualification, QualificationStatus QualificationStatus);

public record ProfessionResponse(JobStatus JobStatus, string Occupation, IncomeRange IncomeRange);

public record ResidencyResponse(string City, string District, string Country);

public record SiblingResponse(SiblingRelationship Relationship, MaritalStatus MaritalStatus, string? Occupation);

public record FamilyResponse(string? FatherOccupation, string? MotherOccupation, int SiblingCount, IReadOnlyList<SiblingResponse> Siblings);

public record LifestyleResponse(LifestyleHabit Smoking, LifestyleHabit Alcohol);

public record AssetsResponse(AssetsStatus Status);

public record VerificationResponse(bool PhoneVerified, bool EmailVerified, bool IdentityVerified, bool PhotoVerified, bool ProfessionVerified, bool EducationVerified);

/// Mirrors the FE's Profile entity field-for-field. IsFavourite/RelationshipStatus are
/// computed relative to the requesting user (ProfileService.GetByIdAsync's viewerUserId),
/// not intrinsic to the profile itself — see ProfileConnectionRequest/ProfileFavourite.
public record ProfileResponse(
    Guid Id,
    Guid UserId,
    string FirstName,
    string LastName,
    IReadOnlyList<PhotoResponse> Photos,
    int Age,
    string City,
    string LastActiveLabel,
    ManagedBy? ManagedBy,
    bool IsFavourite,
    string RelationshipStatus,
    BasicInfoResponse BasicInfo,
    EducationResponse Education,
    ProfessionResponse Profession,
    ResidencyResponse Residency,
    FamilyResponse Family,
    LifestyleResponse Lifestyle,
    AssetsResponse Assets,
    IReadOnlyList<string> Hobbies,
    IReadOnlyList<string> Interests,
    VerificationResponse Verification,
    string? AboutMe,
    bool IsGold);
