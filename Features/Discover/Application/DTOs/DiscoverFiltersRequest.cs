using Wedding_Proposal_BE.Features.Profiles.Domain.Enums;

namespace Wedding_Proposal_BE.Features.Discover.Application.DTOs;

/// Bound via [FromQuery] on DiscoverController. Mirrors the FE's DiscoverFilters field-for-
/// field (see discover/domain/entities/DiscoverFilters.ts) plus server-side Page/PageSize.
public class DiscoverFiltersRequest
{
    public string SortBy { get; set; } = "newest";
    public int MinAge { get; set; } = 18;
    public int MaxAge { get; set; } = 65;
    public List<Gender> Genders { get; set; } = [];
    public string? Country { get; set; }
    public string? District { get; set; }
    public string? City { get; set; }
    public List<string> Religions { get; set; } = [];
    public List<string> Races { get; set; } = [];
    public List<string> Castes { get; set; } = [];
    public List<MaritalStatus> MaritalStatuses { get; set; } = [];
    public string? JobTitle { get; set; }
    public bool OnlyForeign { get; set; }
    public bool OnlyGold { get; set; }
    public bool OnlyWithPhotos { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 14;
}
