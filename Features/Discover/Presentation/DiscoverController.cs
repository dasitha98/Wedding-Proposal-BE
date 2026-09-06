using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Wedding_Proposal_BE.Features.Discover.Application.DTOs;
using Wedding_Proposal_BE.Features.Discover.Application.Interfaces;
using Wedding_Proposal_BE.Shared.Common;

namespace Wedding_Proposal_BE.Features.Discover.Presentation;

[ApiController]
[Route("api/discover")]
[Authorize]
public class DiscoverController : ControllerBase
{
    private readonly IDiscoverService _discoverService;

    public DiscoverController(IDiscoverService discoverService)
    {
        _discoverService = discoverService;
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> Get([FromQuery] DiscoverFiltersRequest filters)
    {
        var result = await _discoverService.GetProfilesAsync(CurrentUserId, filters);
        return result.IsSuccess
            ? Ok(ApiResponse<DiscoverResultResponse>.Ok(result.Value))
            : BadRequest(ApiResponse<DiscoverResultResponse>.Fail(result.Error!, result.ErrorCode));
    }

    // Null for anonymous requests — Discover is browsable without an account.
    private Guid? CurrentUserId =>
        Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var id) ? id : null;
}
