using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Wedding_Proposal_BE.Features.Admin.Application.DTOs;
using Wedding_Proposal_BE.Features.Admin.Application.Interfaces;
using Wedding_Proposal_BE.Shared.Common;
using Wedding_Proposal_BE.Shared.Infrastructure.Identity;

namespace Wedding_Proposal_BE.Features.Admin.Presentation;

[ApiController]
[Route("api/admin/roles")]
[Authorize(Roles = RoleNames.AdminAccess)]
public class AdminRolesController : ControllerBase
{
    private readonly IAdminRoleService _service;

    public AdminRolesController(IAdminRoleService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<IActionResult> List()
    {
        var result = await _service.ListAsync();
        return Ok(ApiResponse<IReadOnlyList<AdminRoleDto>>.Ok(result.Value));
    }
}
