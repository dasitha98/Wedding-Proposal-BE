using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Wedding_Proposal_BE.Features.Admin.Application.DTOs;
using Wedding_Proposal_BE.Features.Admin.Application.Interfaces;
using Wedding_Proposal_BE.Shared.Common;
using Wedding_Proposal_BE.Shared.Infrastructure.Identity;

namespace Wedding_Proposal_BE.Features.Admin.Presentation;

[ApiController]
[Route("api/admin")]
[Authorize(Roles = RoleNames.AdminAccess)]
public class AdminSecurityController : ControllerBase
{
    private readonly IAdminSecurityService _service;

    public AdminSecurityController(IAdminSecurityService service)
    {
        _service = service;
    }

    [HttpGet("refresh-tokens")]
    public async Task<IActionResult> ListRefreshTokens([FromQuery] AdminListRequest request)
    {
        var result = await _service.ListRefreshTokensAsync(request.Page, request.PageSize);
        return Ok(ApiResponse<AdminPagedResponse<AdminRefreshTokenDto>>.Ok(result.Value));
    }

    [HttpDelete("refresh-tokens/{id:guid}")]
    public async Task<IActionResult> DeleteRefreshToken(Guid id)
    {
        var result = await _service.DeleteRefreshTokenAsync(id);
        return result.IsSuccess ? Ok(ApiResponse.Ok()) : BadRequest(ApiResponse.Fail(result.Error!, result.ErrorCode));
    }

    [HttpGet("otps/password-reset")]
    public async Task<IActionResult> ListPasswordResetOtps([FromQuery] AdminListRequest request)
    {
        var result = await _service.ListPasswordResetOtpsAsync(request.Page, request.PageSize);
        return Ok(ApiResponse<AdminPagedResponse<AdminOtpDto>>.Ok(result.Value));
    }

    [HttpDelete("otps/password-reset/{id:guid}")]
    public async Task<IActionResult> DeletePasswordResetOtp(Guid id)
    {
        var result = await _service.DeletePasswordResetOtpAsync(id);
        return result.IsSuccess ? Ok(ApiResponse.Ok()) : BadRequest(ApiResponse.Fail(result.Error!, result.ErrorCode));
    }

    [HttpGet("otps/email-verification")]
    public async Task<IActionResult> ListEmailVerificationOtps([FromQuery] AdminListRequest request)
    {
        var result = await _service.ListEmailVerificationOtpsAsync(request.Page, request.PageSize);
        return Ok(ApiResponse<AdminPagedResponse<AdminOtpDto>>.Ok(result.Value));
    }

    [HttpDelete("otps/email-verification/{id:guid}")]
    public async Task<IActionResult> DeleteEmailVerificationOtp(Guid id)
    {
        var result = await _service.DeleteEmailVerificationOtpAsync(id);
        return result.IsSuccess ? Ok(ApiResponse.Ok()) : BadRequest(ApiResponse.Fail(result.Error!, result.ErrorCode));
    }
}
