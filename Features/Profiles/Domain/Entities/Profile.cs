using Wedding_Proposal_BE.Features.Profiles.Domain.Enums;
using Wedding_Proposal_BE.Features.Profiles.Domain.ValueObjects;

namespace Wedding_Proposal_BE.Features.Profiles.Domain.Entities;

/// Matrimonial profile for a user. Kept separate from ApplicationUser (Identity) so identity
/// concerns and profile/domain concerns don't bleed into one class. Composed of owned value
/// objects (BasicInfo, Education, Profession, Residency, Family, Lifestyle, Assets,
/// Verification) mirroring the FE's sectioned Profile entity, plus the Siblings/Photos child
/// collections. RelationshipStatus/IsFavourite are intentionally NOT stored here — they are
/// per-viewer and derived from ProfileConnectionRequest/ProfileFavourite by ProfileService.
public class Profile
{
    private readonly List<Sibling> _siblings = [];
    private readonly List<ProfilePhoto> _photos = [];

    public Guid Id { get; private set; }
    public Guid UserId { get; private set; }
    public string FirstName { get; private set; } = string.Empty;
    public string LastName { get; private set; } = string.Empty;
    public DateOnly DateOfBirth { get; private set; }
    public ManagedBy? ManagedBy { get; private set; }
    public string? AboutMe { get; private set; }
    public List<string> Hobbies { get; private set; } = [];
    public List<string> Interests { get; private set; } = [];
    public bool IsGold { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    public BasicInfo BasicInfo { get; private set; } = null!;
    public Education Education { get; private set; } = null!;
    public Profession Profession { get; private set; } = null!;
    public Residency Residency { get; private set; } = null!;
    public Family Family { get; private set; } = null!;
    public Lifestyle Lifestyle { get; private set; } = null!;
    public Assets Assets { get; private set; } = null!;
    public Verification Verification { get; private set; } = null!;

    public IReadOnlyCollection<Sibling> Siblings => _siblings;
    public IReadOnlyCollection<ProfilePhoto> Photos => _photos;

    private Profile()
    {
    }

    public static Profile CreateBlank(Guid userId, string firstName, string lastName, DateOnly dateOfBirth)
    {
        var now = DateTime.UtcNow;
        return new Profile
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            FirstName = firstName,
            LastName = lastName,
            DateOfBirth = dateOfBirth,
            ManagedBy = Enums.ManagedBy.Self,
            Hobbies = [],
            Interests = [],
            IsGold = false,
            CreatedAt = now,
            UpdatedAt = now,
            BasicInfo = BasicInfo.CreateDefault(),
            Education = Education.CreateDefault(),
            Profession = Profession.CreateDefault(),
            Residency = Residency.CreateDefault(),
            Family = Family.CreateDefault(),
            Lifestyle = Lifestyle.CreateDefault(),
            Assets = Assets.CreateDefault(),
            Verification = Verification.CreateDefault()
        };
    }

    public void UpdateDateOfBirth(DateOnly dateOfBirth)
    {
        DateOfBirth = dateOfBirth;
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateBasics(string firstName, string lastName, ManagedBy? managedBy, string? aboutMe, List<string> hobbies, List<string> interests)
    {
        FirstName = firstName;
        LastName = lastName;
        ManagedBy = managedBy;
        AboutMe = aboutMe;
        Hobbies = hobbies;
        Interests = interests;
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateBasicInfo(BasicInfo basicInfo)
    {
        BasicInfo = basicInfo;
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateEducation(Education education)
    {
        Education = education;
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateProfession(Profession profession)
    {
        Profession = profession;
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateResidency(Residency residency)
    {
        Residency = residency;
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateFamily(Family family, IEnumerable<(SiblingRelationship Relationship, MaritalStatus MaritalStatus, string? Occupation)> siblings)
    {
        Family = family;
        _siblings.Clear();
        _siblings.AddRange(siblings.Select(s => Sibling.Create(Id, s.Relationship, s.MaritalStatus, s.Occupation)));
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateLifestyle(Lifestyle lifestyle)
    {
        Lifestyle = lifestyle;
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateAssets(Assets assets)
    {
        Assets = assets;
        UpdatedAt = DateTime.UtcNow;
    }

    public void ReplacePhotos(IEnumerable<(string Uri, bool IsBlurred)> photos)
    {
        _photos.Clear();
        var order = 0;
        foreach (var photo in photos)
        {
            _photos.Add(ProfilePhoto.Create(Id, photo.Uri, photo.IsBlurred, order));
            order++;
        }

        UpdatedAt = DateTime.UtcNow;
    }

    public void SyncEmailVerified(bool isVerified) => Verification.SyncEmailVerified(isVerified);

    /// For system/admin use only (e.g. dev seeding, a future billing/verification workflow) —
    /// never bound to UpdateProfileRequest.
    public void UpdateVerification(Verification verification)
    {
        Verification = verification;
        UpdatedAt = DateTime.UtcNow;
    }

    public void SetGoldStatus(bool isGold)
    {
        IsGold = isGold;
        UpdatedAt = DateTime.UtcNow;
    }

    public int CalculateAge(DateOnly? asOf = null)
    {
        var today = asOf ?? DateOnly.FromDateTime(DateTime.UtcNow);
        var age = today.Year - DateOfBirth.Year;
        if (DateOfBirth > today.AddYears(-age))
        {
            age--;
        }

        return age;
    }

    public string CalculateMaskedDateOfBirth() => $"{DateOfBirth:yyyy-MM}-**";
}
