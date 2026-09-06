using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Wedding_Proposal_BE.Features.Admin.Application.DTOs;
using Wedding_Proposal_BE.Features.Admin.Application.Interfaces;
using Wedding_Proposal_BE.Shared.Common;
using Wedding_Proposal_BE.Shared.Infrastructure.Identity;

namespace Wedding_Proposal_BE.Features.Admin.Presentation;

[ApiController]
[Route("api/admin/users")]
[Authorize(Roles = RoleNames.AdminAccess)]
public class AdminUsersController : ControllerBase
{
    private readonly IAdminUserService _service;

    public AdminUsersController(IAdminUserService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<IActionResult> List([FromQuery] AdminListRequest request)
    {
        var result = await _service.ListAsync(request.Search, request.Page, request.PageSize);
        return Ok(ApiResponse<AdminPagedResponse<AdminUserDto>>.Ok(result.Value));
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] AdminCreateUserRequest request)
    {
        var result = await _service.CreateAsync(request);
        return result.IsSuccess
            ? Ok(ApiResponse<AdminUserDto>.Ok(result.Value))
            : BadRequest(ApiResponse<AdminUserDto>.Fail(result.Error!, result.ErrorCode));
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await _service.GetByIdAsync(id);
        return result.IsSuccess
            ? Ok(ApiResponse<AdminUserDto>.Ok(result.Value))
            : NotFound(ApiResponse<AdminUserDto>.Fail(result.Error!, result.ErrorCode));
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] AdminUpdateUserRequest request)
    {
        var result = await _service.UpdateAsync(id, request);
        return result.IsSuccess
            ? Ok(ApiResponse<AdminUserDto>.Ok(result.Value))
            : BadRequest(ApiResponse<AdminUserDto>.Fail(result.Error!, result.ErrorCode));
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var result = await _service.DeleteAsync(id);
        return result.IsSuccess
            ? Ok(ApiResponse.Ok())
            : BadRequest(ApiResponse.Fail(result.Error!, result.ErrorCode));
    }
}
