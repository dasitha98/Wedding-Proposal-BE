using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Wedding_Proposal_BE.Features.Messages.Application.DTOs;
using Wedding_Proposal_BE.Features.Messages.Application.Interfaces;
using Wedding_Proposal_BE.Shared.Common;

namespace Wedding_Proposal_BE.Features.Messages.Presentation;

[ApiController]
[Route("api/messages")]
[Authorize]
public class MessagesController : ControllerBase
{
    private readonly IMessageService _messageService;

    public MessagesController(IMessageService messageService)
    {
        _messageService = messageService;
    }

    [HttpGet("conversations")]
    public async Task<IActionResult> GetConversations()
    {
        var result = await _messageService.GetConversationsAsync(CurrentUserId);
        return Ok(ApiResponse<IReadOnlyList<ConversationSummaryResponse>>.Ok(result.Value));
    }

    [HttpGet("conversations/{profileId:guid}")]
    public async Task<IActionResult> GetConversation(Guid profileId)
    {
        var result = await _messageService.GetConversationAsync(CurrentUserId, profileId);
        return result.IsSuccess
            ? Ok(ApiResponse<IReadOnlyList<MessageResponse>>.Ok(result.Value))
            : NotFound(ApiResponse<IReadOnlyList<MessageResponse>>.Fail(result.Error!, result.ErrorCode));
    }

    [HttpPost("conversations/{profileId:guid}")]
    public async Task<IActionResult> SendMessage(Guid profileId, [FromBody] SendMessageRequest request)
    {
        var result = await _messageService.SendMessageAsync(CurrentUserId, profileId, request.Text);
        return result.IsSuccess
            ? Ok(ApiResponse<MessageResponse>.Ok(result.Value))
            : BadRequest(ApiResponse<MessageResponse>.Fail(result.Error!, result.ErrorCode));
    }

    private Guid CurrentUserId => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
}
