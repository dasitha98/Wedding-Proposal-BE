namespace Wedding_Proposal_BE.Features.Users.Application.DTOs;

public record UserResponse(Guid Id, string FirstName, string LastName, string Email, DateTime CreatedAt);
