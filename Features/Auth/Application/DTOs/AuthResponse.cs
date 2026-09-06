namespace Wedding_Proposal_BE.Features.Auth.Application.DTOs;

public record AuthUserResponse(Guid Id, string FirstName, string LastName, string Email, bool IsEmailVerified, IReadOnlyCollection<string> Roles);

public record AuthResponse(
    string AccessToken,
    DateTime AccessTokenExpiresAt,
    string RefreshToken,
    AuthUserResponse User);
