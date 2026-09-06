using Wedding_Proposal_BE.Features.Profiles.Application.DTOs;
using Wedding_Proposal_BE.Features.Profiles.Domain.Enums;

namespace Wedding_Proposal_BE.Features.Admin.Application.DTOs;

public record AdminProfileSummaryDto(
    Guid Id, Guid UserId, string FullName, string Email, Gender Gender, int Age,
    string Religion, string City, string Country, bool IsGold, int PhotoCount, DateTime CreatedAt);

public record AdminBasicInfoDto(Gender Gender, string Race, string Religion, string Caste, MaritalStatus MaritalStatus, string HeightLabel);
public record AdminEducationDto(EducationStatus Qualification, QualificationStatus QualificationStatus);
public record AdminProfessionDto(JobStatus JobStatus, string Occupation, IncomeRange IncomeRange);
public record AdminResidencyDto(string City, string District, string Country);
public record AdminFamilyDto(string? FatherOccupation, string? MotherOccupation, int SiblingCount);
public record AdminLifestyleDto(LifestyleHabit Smoking, LifestyleHabit Alcohol);
public record AdminAssetsDto(AssetsStatus Status);
public record AdminVerificationDto(
    bool PhoneVerified, bool EmailVerified, bool IdentityVerified,
    bool PhotoVerified, bool ProfessionVerified, bool EducationVerified);
public record AdminPhotoDto(Guid Id, string Uri, bool IsBlurred, int Order);
public record AdminSiblingDto(Guid Id, SiblingRelationship Relationship, MaritalStatus MaritalStatus, string? Occupation);

public record AdminProfileDetailDto(
    Guid Id, Guid UserId, string FirstName, string LastName, string Email, DateOnly DateOfBirth, int Age,
    ManagedBy? ManagedBy, string? AboutMe, IReadOnlyList<string> Hobbies, IReadOnlyList<string> Interests, bool IsGold,
    DateTime CreatedAt, DateTime UpdatedAt,
    AdminBasicInfoDto BasicInfo, AdminEducationDto Education, AdminProfessionDto Profession,
    AdminResidencyDto Residency, AdminFamilyDto Family, AdminLifestyleDto Lifestyle, AdminAssetsDto Assets,
    AdminVerificationDto Verification, IReadOnlyList<AdminPhotoDto> Photos, IReadOnlyList<AdminSiblingDto> Siblings);

/// Every field is optional: a null section means "leave unchanged" (same patch semantics as
/// the self-service UpdateProfileRequest). Admins can edit the full profile, including the
/// self-reported sections, on top of the admin-only IsGold/Verification fields.
public class AdminUpdateProfileRequest
{
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public DateOnly? DateOfBirth { get; set; }
    public ManagedBy? ManagedBy { get; set; }
    public string? AboutMe { get; set; }
    public List<string>? Hobbies { get; set; }
    public List<string>? Interests { get; set; }
    public List<PhotoUpdate>? Photos { get; set; }

    public BasicInfoUpdate? BasicInfo { get; set; }
    public EducationUpdate? Education { get; set; }
    public ProfessionUpdate? Profession { get; set; }
    public ResidencyUpdate? Residency { get; set; }
    public FamilyUpdate? Family { get; set; }
    public LifestyleUpdate? Lifestyle { get; set; }
    public AssetsUpdate? Assets { get; set; }

    public bool? IsGold { get; set; }
    public AdminVerificationDto? Verification { get; set; }
}

/// Admin creates a blank profile on behalf of an existing account (one that hasn't made one
/// itself yet) — mirrors self-service CreateProfileRequest but targets an arbitrary UserId
/// instead of the caller.
public class AdminCreateProfileRequest
{
    public Guid UserId { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public DateOnly DateOfBirth { get; set; }
}
