using Wedding_Proposal_BE.Features.Profiles.Domain.Enums;

namespace Wedding_Proposal_BE.Features.Discover.Application.DTOs;

/// Denormalized card-view projection over Profile — mirrors discover/domain/entities/
/// Profile.ts (distinct from and thinner than ProfileResponse, the detail view).
public record DiscoverProfileResponse(
    Guid Id,
    string Name,
    int Age,
    Gender Gender,
    string? Bio,
    string Country,
    string? District,
    string? City,
    string Religion,
    string Race,
    string? Caste,
    MaritalStatus MaritalStatus,
    string JobTitle,
    DateTime JoinedAt,
    bool IsGold,
    IReadOnlyList<string> Photos,
    IReadOnlyList<string> Interests,
    bool IsVerified);

public record DiscoverResultResponse(
    IReadOnlyList<DiscoverProfileResponse> Items,
    int TotalCount,
    int TotalPages,
    int Page,
    int PageSize);
