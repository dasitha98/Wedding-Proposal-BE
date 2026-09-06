using Microsoft.AspNetCore.Identity;

namespace Wedding_Proposal_BE.Shared.Infrastructure.Identity;

/// Placeholder Identity user. Extended user/profile data belongs to the Users/Profiles features,
/// not here — this class only carries what Identity and Auth need.
public class ApplicationUser : IdentityUser<Guid>
{
    public string FirstName { get; private set; } = string.Empty;
    public string LastName { get; private set; } = string.Empty;
    public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; private set; } = DateTime.UtcNow;

    /// Last time this user made an authenticated request. Null until their first one.
    /// Written directly via EF's ExecuteUpdate by LastActiveTrackingMiddleware, not through
    /// this entity, so no setter-invoking method is exposed here.
    public DateTime? LastActiveAt { get; private set; }

    /// Google's stable, unique end-user identifier (the ID token's `sub` claim). Null for
    /// accounts that have only ever signed in with a password. Never reused across Google
    /// accounts, so this — not email — is the identity key for Google sign-in.
    public string? GoogleSubject { get; private set; }

    /// Display name as reported by Google (the ID token's `name` claim). Distinct from
    /// FirstName/LastName, which remain the app's own editable profile fields.
    public string? Name { get; private set; }

    public string? ProfileImageUrl { get; private set; }

    public static ApplicationUser Create(string email, string firstName, string lastName)
    {
        var now = DateTime.UtcNow;
        return new ApplicationUser
        {
            Id = Guid.NewGuid(),
            Email = email,
            UserName = email,
            FirstName = firstName,
            LastName = lastName,
            CreatedAt = now,
            UpdatedAt = now
        };
    }

    public static ApplicationUser CreateFromGoogle(
        string googleSubject, string email, bool emailVerified, string? name, string? firstName, string? lastName, string? profileImageUrl)
    {
        var now = DateTime.UtcNow;
        return new ApplicationUser
        {
            Id = Guid.NewGuid(),
            Email = email,
            UserName = email,
            EmailConfirmed = emailVerified,
            FirstName = firstName ?? name ?? "Google",
            LastName = lastName ?? string.Empty,
            GoogleSubject = googleSubject,
            Name = name,
            ProfileImageUrl = profileImageUrl,
            CreatedAt = now,
            UpdatedAt = now
        };
    }

    public void UpdateName(string firstName, string lastName)
    {
        FirstName = firstName;
        LastName = lastName;
        UpdatedAt = DateTime.UtcNow;
    }

    /// Attaches a Google identity to an account that previously only had a password — only
    /// ever called after Google has confirmed ownership of this account's email address.
    public void LinkGoogleAccount(string googleSubject, string? name, string? profileImageUrl)
    {
        GoogleSubject = googleSubject;
        Name = name ?? Name;
        ProfileImageUrl = profileImageUrl ?? ProfileImageUrl;
        UpdatedAt = DateTime.UtcNow;
    }

    /// Refreshes the Google-sourced profile fields on every Google sign-in, in case the user
    /// changed their name/photo on their Google account since the last sign-in.
    public void UpdateGoogleProfile(string? name, string? profileImageUrl)
    {
        Name = name ?? Name;
        ProfileImageUrl = profileImageUrl ?? ProfileImageUrl;
        UpdatedAt = DateTime.UtcNow;
    }
}
