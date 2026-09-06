namespace Wedding_Proposal_BE.Features.Admin.Application.DTOs;

public record AdminPagedResponse<T>(IReadOnlyList<T> Items, int TotalCount, int TotalPages, int Page, int PageSize);

/// Bound via [FromQuery] on admin list endpoints.
public class AdminListRequest
{
    public string? Search { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}
