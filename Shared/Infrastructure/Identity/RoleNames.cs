namespace Wedding_Proposal_BE.Shared.Infrastructure.Identity;

/// The fixed set of roles this app supports. Seeded at startup (see Program.cs) and used
/// wherever code needs to refer to one of them by name instead of a magic string.
public static class RoleNames
{
    public const string SuperAdmin = "SuperAdmin";
    public const string Admin = "Admin";
    public const string User = "User";

    public static readonly IReadOnlyList<string> All = [SuperAdmin, Admin, User];

    /// For [Authorize(Roles = ...)] on admin-only endpoints — ASP.NET Core treats a
    /// comma-separated Roles string as "any of these", not "all of these".
    public const string AdminAccess = $"{Admin},{SuperAdmin}";
}
