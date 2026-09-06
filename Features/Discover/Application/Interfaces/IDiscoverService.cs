using Wedding_Proposal_BE.Features.Discover.Application.DTOs;
using Wedding_Proposal_BE.Shared.Common;

namespace Wedding_Proposal_BE.Features.Discover.Application.Interfaces;

public interface IDiscoverService
{
    Task<Result<DiscoverResultResponse>> GetProfilesAsync(Guid? viewerUserId, DiscoverFiltersRequest filters);
}
