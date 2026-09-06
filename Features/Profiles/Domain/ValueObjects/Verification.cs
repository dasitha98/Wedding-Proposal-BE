namespace Wedding_Proposal_BE.Features.Profiles.Domain.ValueObjects;

/// System-computed — never accepted from client update requests. EmailVerified is derived
/// from the account's Identity EmailConfirmed flag; the rest default false until a
/// verification feature exists to set them (see ProfileService.SyncVerification).
public class Verification
{
    public bool PhoneVerified { get; private set; }
    public bool EmailVerified { get; private set; }
    public bool IdentityVerified { get; private set; }
    public bool PhotoVerified { get; private set; }
    public bool ProfessionVerified { get; private set; }
    public bool EducationVerified { get; private set; }

    private Verification()
    {
    }

    public static Verification CreateDefault() => new();

    /// For system/admin use only (e.g. dev seeding, a future verification workflow) — never
    /// bind this directly to a client request.
    public static Verification Create(bool phoneVerified, bool emailVerified, bool identityVerified, bool photoVerified, bool professionVerified, bool educationVerified) =>
        new()
        {
            PhoneVerified = phoneVerified,
            EmailVerified = emailVerified,
            IdentityVerified = identityVerified,
            PhotoVerified = photoVerified,
            ProfessionVerified = professionVerified,
            EducationVerified = educationVerified
        };

    public void SyncEmailVerified(bool isVerified) => EmailVerified = isVerified;
}
