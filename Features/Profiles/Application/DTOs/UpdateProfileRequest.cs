using Wedding_Proposal_BE.Features.Profiles.Domain.Enums;

namespace Wedding_Proposal_BE.Features.Profiles.Application.DTOs;

/// Every top-level and nested-object field is optional: a null section means "leave
/// unchanged," matching the FE's MyProfileUpdateInput patch semantics (partial merge, not
/// wholesale replace) — see ProfileService.UpdateAsync. Verification/IsFavourite/
/// RelationshipStatus are intentionally absent: they are system-computed, never client-set.
public class UpdateProfileRequest
{
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    /// Not part of the FE's MyProfileUpdateInput mock shape, but needed in practice: the blank
    /// profile created on first access has a placeholder date of birth (see ProfileService.
    /// CreateBlankOnFirstAccess) that the user must be able to correct afterwards.
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
}

public class BasicInfoUpdate
{
    public Gender? Gender { get; set; }
    public string? Race { get; set; }
    public string? Religion { get; set; }
    public string? Caste { get; set; }
    public MaritalStatus? MaritalStatus { get; set; }
    public string? HeightLabel { get; set; }
}

public class EducationUpdate
{
    public EducationStatus? Qualification { get; set; }
    public QualificationStatus? QualificationStatus { get; set; }
}

public class ProfessionUpdate
{
    public JobStatus? JobStatus { get; set; }
    public string? Occupation { get; set; }
    public IncomeRange? IncomeRange { get; set; }
}

public class ResidencyUpdate
{
    public string? City { get; set; }
    public string? District { get; set; }
    public string? Country { get; set; }
}

public class FamilyUpdate
{
    public string? FatherOccupation { get; set; }
    public string? MotherOccupation { get; set; }
    public int? SiblingCount { get; set; }

    /// When provided, fully replaces the sibling record list (matches FE semantics).
    public List<SiblingUpdate>? Siblings { get; set; }
}

public class SiblingUpdate
{
    public SiblingRelationship Relationship { get; set; }
    public MaritalStatus MaritalStatus { get; set; }
    public string? Occupation { get; set; }
}

public class LifestyleUpdate
{
    public LifestyleHabit? Smoking { get; set; }
    public LifestyleHabit? Alcohol { get; set; }
}

public class AssetsUpdate
{
    public AssetsStatus? Status { get; set; }
}

public class PhotoUpdate
{
    public string Uri { get; set; } = string.Empty;
    public bool IsBlurred { get; set; }
}
