using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Wedding_Proposal_BE.Features.Users.Application.DTOs;
using Wedding_Proposal_BE.Features.Users.Application.Interfaces;
using Wedding_Proposal_BE.Shared.Common;

namespace Wedding_Proposal_BE.Features.Users.Presentation;

[ApiController]
[Route("api/users")]
[Authorize]
public class UsersController : ControllerBase
{
    private readonly IUserService _userService;

    public UsersController(IUserService userService)
    {
        _userService = userService;
    }

    [HttpGet("me")]
    public async Task<IActionResult> GetCurrentUser()
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var result = await _userService.GetByIdAsync(userId);
        return result.IsSuccess
            ? Ok(ApiResponse<UserResponse>.Ok(result.Value))
            : NotFound(ApiResponse<UserResponse>.Fail(result.Error!, result.ErrorCode));
    }
}
