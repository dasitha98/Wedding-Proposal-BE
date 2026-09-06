using Wedding_Proposal_BE.Features.Admin.Application.DTOs;
using Wedding_Proposal_BE.Features.Profiles.Domain.Enums;
using Wedding_Proposal_BE.Shared.Common;

namespace Wedding_Proposal_BE.Features.Admin.Application.Interfaces;

public interface IAdminEngagementService
{
    Task<Result<AdminPagedResponse<AdminConnectionRequestDto>>> ListConnectionRequestsAsync(int page, int pageSize);
    Task<Result<AdminConnectionRequestDto>> UpdateConnectionRequestStatusAsync(Guid id, ConnectionStatus status);
    Task<Result> DeleteConnectionRequestAsync(Guid id);

    Task<Result<AdminPagedResponse<AdminFavouriteDto>>> ListFavouritesAsync(int page, int pageSize);
    Task<Result> DeleteFavouriteAsync(Guid id);

    Task<Result<AdminPagedResponse<AdminConversationDto>>> ListConversationsAsync(int page, int pageSize);
    Task<Result> DeleteConversationAsync(Guid id);

    Task<Result<AdminPagedResponse<AdminMessageDto>>> ListMessagesAsync(Guid conversationId, int page, int pageSize);
    Task<Result> DeleteMessageAsync(Guid id);
}
