namespace Wedding_Proposal_BE.Features.Admin.Application.DTOs;

public record AdminUserDto(
    Guid Id,
    string FirstName,
    string LastName,
    string Email,
    bool EmailConfirmed,
    bool IsLockedOut,
    DateTime CreatedAt,
    DateTime? LastActiveAt,
    IReadOnlyList<string> Roles,
    Guid? ProfileId);

public class AdminUpdateUserRequest
{
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? Email { get; set; }
    public bool? EmailConfirmed { get; set; }
    public bool? IsLockedOut { get; set; }
    public List<string>? Roles { get; set; }
}

/// Admin-created accounts skip the OTP flow entirely (EmailConfirmed is set immediately) —
/// the admin creating it has already vouched for the email, unlike self-registration.
public class AdminCreateUserRequest
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public List<string>? Roles { get; set; }
}
