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
public class AdminEngagementController : ControllerBase
{
    private readonly IAdminEngagementService _service;

    public AdminEngagementController(IAdminEngagementService service)
    {
        _service = service;
    }

    [HttpGet("connection-requests")]
    public async Task<IActionResult> ListConnectionRequests([FromQuery] AdminListRequest request)
    {
        var result = await _service.ListConnectionRequestsAsync(request.Page, request.PageSize);
        return Ok(ApiResponse<AdminPagedResponse<AdminConnectionRequestDto>>.Ok(result.Value));
    }

    [HttpPut("connection-requests/{id:guid}/status")]
    public async Task<IActionResult> UpdateConnectionRequestStatus(Guid id, [FromBody] AdminUpdateConnectionRequestStatusRequest request)
    {
        var result = await _service.UpdateConnectionRequestStatusAsync(id, request.Status);
        return result.IsSuccess
            ? Ok(ApiResponse<AdminConnectionRequestDto>.Ok(result.Value))
            : BadRequest(ApiResponse<AdminConnectionRequestDto>.Fail(result.Error!, result.ErrorCode));
    }

    [HttpDelete("connection-requests/{id:guid}")]
    public async Task<IActionResult> DeleteConnectionRequest(Guid id)
    {
        var result = await _service.DeleteConnectionRequestAsync(id);
        return result.IsSuccess ? Ok(ApiResponse.Ok()) : BadRequest(ApiResponse.Fail(result.Error!, result.ErrorCode));
    }

    [HttpGet("favourites")]
    public async Task<IActionResult> ListFavourites([FromQuery] AdminListRequest request)
    {
        var result = await _service.ListFavouritesAsync(request.Page, request.PageSize);
        return Ok(ApiResponse<AdminPagedResponse<AdminFavouriteDto>>.Ok(result.Value));
    }

    [HttpDelete("favourites/{id:guid}")]
    public async Task<IActionResult> DeleteFavourite(Guid id)
    {
        var result = await _service.DeleteFavouriteAsync(id);
        return result.IsSuccess ? Ok(ApiResponse.Ok()) : BadRequest(ApiResponse.Fail(result.Error!, result.ErrorCode));
    }

    [HttpGet("conversations")]
    public async Task<IActionResult> ListConversations([FromQuery] AdminListRequest request)
    {
        var result = await _service.ListConversationsAsync(request.Page, request.PageSize);
        return Ok(ApiResponse<AdminPagedResponse<AdminConversationDto>>.Ok(result.Value));
    }

    [HttpDelete("conversations/{id:guid}")]
    public async Task<IActionResult> DeleteConversation(Guid id)
    {
        var result = await _service.DeleteConversationAsync(id);
        return result.IsSuccess ? Ok(ApiResponse.Ok()) : BadRequest(ApiResponse.Fail(result.Error!, result.ErrorCode));
    }

    [HttpGet("conversations/{conversationId:guid}/messages")]
    public async Task<IActionResult> ListMessages(Guid conversationId, [FromQuery] AdminListRequest request)
    {
        var result = await _service.ListMessagesAsync(conversationId, request.Page, request.PageSize);
        return Ok(ApiResponse<AdminPagedResponse<AdminMessageDto>>.Ok(result.Value));
    }

    [HttpDelete("messages/{id:guid}")]
    public async Task<IActionResult> DeleteMessage(Guid id)
    {
        var result = await _service.DeleteMessageAsync(id);
        return result.IsSuccess ? Ok(ApiResponse.Ok()) : BadRequest(ApiResponse.Fail(result.Error!, result.ErrorCode));
    }
}
