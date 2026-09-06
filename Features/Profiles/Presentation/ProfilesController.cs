using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Wedding_Proposal_BE.Features.Profiles.Application.DTOs;
using Wedding_Proposal_BE.Features.Profiles.Application.Interfaces;
using Wedding_Proposal_BE.Shared.Common;

namespace Wedding_Proposal_BE.Features.Profiles.Presentation;

[ApiController]
[Route("api/profiles")]
[Authorize]
public class ProfilesController : ControllerBase
{
    private readonly IProfileService _profileService;

    public ProfilesController(IProfileService profileService)
    {
        _profileService = profileService;
    }

    [HttpGet("me")]
    public async Task<IActionResult> GetMine()
    {
        var result = await _profileService.GetByUserIdAsync(CurrentUserId);
        return result.IsSuccess
            ? Ok(ApiResponse<ProfileResponse>.Ok(result.Value))
            : NotFound(ApiResponse<ProfileResponse>.Fail(result.Error!, result.ErrorCode));
    }

    [HttpGet("{id:guid}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await _profileService.GetByIdAsync(id, CurrentUserIdOrNull);
        return result.IsSuccess
            ? Ok(ApiResponse<ProfileResponse>.Ok(result.Value))
            : NotFound(ApiResponse<ProfileResponse>.Fail(result.Error!, result.ErrorCode));
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateProfileRequest request)
    {
        var result = await _profileService.CreateAsync(CurrentUserId, request);
        return result.IsSuccess
            ? Ok(ApiResponse<ProfileResponse>.Ok(result.Value))
            : BadRequest(ApiResponse<ProfileResponse>.Fail(result.Error!, result.ErrorCode));
    }

    [HttpPut("me")]
    public async Task<IActionResult> Update([FromBody] UpdateProfileRequest request)
    {
        var result = await _profileService.UpdateAsync(CurrentUserId, request);
        return result.IsSuccess
            ? Ok(ApiResponse<ProfileResponse>.Ok(result.Value))
            : BadRequest(ApiResponse<ProfileResponse>.Fail(result.Error!, result.ErrorCode));
    }

    [HttpPost("me/photos")]
    [RequestSizeLimit(5 * 1024 * 1024)]
    public async Task<IActionResult> UploadPhoto(IFormFile file)
    {
        var result = await _profileService.UploadPhotoAsync(CurrentUserId, file);
        return result.IsSuccess
            ? Ok(ApiResponse<PhotoUploadResponse>.Ok(result.Value))
            : BadRequest(ApiResponse<PhotoUploadResponse>.Fail(result.Error!, result.ErrorCode));
    }

    [HttpPost("{id:guid}/requests")]
    public async Task<IActionResult> SendRequest(Guid id)
    {
        var result = await _profileService.SendConnectionRequestAsync(id, CurrentUserId);
        return result.IsSuccess
            ? Ok(ApiResponse<ProfileResponse>.Ok(result.Value))
            : BadRequest(ApiResponse<ProfileResponse>.Fail(result.Error!, result.ErrorCode));
    }

    [HttpDelete("{id:guid}/requests")]
    public async Task<IActionResult> CancelRequest(Guid id)
    {
        var result = await _profileService.CancelConnectionRequestAsync(id, CurrentUserId);
        return result.IsSuccess
            ? Ok(ApiResponse<ProfileResponse>.Ok(result.Value))
            : BadRequest(ApiResponse<ProfileResponse>.Fail(result.Error!, result.ErrorCode));
    }

    [HttpPost("{id:guid}/favourite")]
    public async Task<IActionResult> ToggleFavourite(Guid id)
    {
        var result = await _profileService.ToggleFavouriteAsync(id, CurrentUserId);
        return result.IsSuccess
            ? Ok(ApiResponse<ProfileResponse>.Ok(result.Value))
            : BadRequest(ApiResponse<ProfileResponse>.Fail(result.Error!, result.ErrorCode));
    }

    [HttpGet("requests/sent")]
    public async Task<IActionResult> GetSentRequests()
    {
        var result = await _profileService.GetSentRequestsAsync(CurrentUserId);
        return Ok(ApiResponse<IReadOnlyList<ConnectionRequestResponse>>.Ok(result.Value));
    }

    [HttpGet("requests/received")]
    public async Task<IActionResult> GetReceivedRequests()
    {
        var result = await _profileService.GetReceivedRequestsAsync(CurrentUserId);
        return Ok(ApiResponse<IReadOnlyList<ConnectionRequestResponse>>.Ok(result.Value));
    }

    [HttpPost("requests/{requestId:guid}/accept")]
    public async Task<IActionResult> AcceptRequest(Guid requestId)
    {
        var result = await _profileService.AcceptRequestAsync(requestId, CurrentUserId);
        return result.IsSuccess
            ? Ok(ApiResponse<ConnectionRequestResponse>.Ok(result.Value))
            : BadRequest(ApiResponse<ConnectionRequestResponse>.Fail(result.Error!, result.ErrorCode));
    }

    [HttpPost("requests/{requestId:guid}/decline")]
    public async Task<IActionResult> DeclineRequest(Guid requestId)
    {
        var result = await _profileService.DeclineRequestAsync(requestId, CurrentUserId);
        return result.IsSuccess
            ? Ok(ApiResponse<ConnectionRequestResponse>.Ok(result.Value))
            : BadRequest(ApiResponse<ConnectionRequestResponse>.Fail(result.Error!, result.ErrorCode));
    }

    [HttpGet("favourites")]
    public async Task<IActionResult> GetFavourites()
    {
        var result = await _profileService.GetFavouritesAsync(CurrentUserId);
        return Ok(ApiResponse<IReadOnlyList<ProfileSummaryResponse>>.Ok(result.Value));
    }

    private Guid CurrentUserId => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    // Null for anonymous requests — GetById is browsable without an account.
    private Guid? CurrentUserIdOrNull =>
        Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var id) ? id : null;
}
