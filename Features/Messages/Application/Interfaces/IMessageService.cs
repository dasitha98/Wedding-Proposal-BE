using Wedding_Proposal_BE.Features.Messages.Application.DTOs;
using Wedding_Proposal_BE.Shared.Common;

namespace Wedding_Proposal_BE.Features.Messages.Application.Interfaces;

public interface IMessageService
{
    /// profileId identifies the other party's Profile (keeps the FE's existing profile-ID-
    /// keyed contract) — resolved internally to the owning user's conversation.
    Task<Result<IReadOnlyList<MessageResponse>>> GetConversationAsync(Guid currentUserId, Guid profileId);

    Task<Result<MessageResponse>> SendMessageAsync(Guid currentUserId, Guid profileId, string text);

    Task<Result<IReadOnlyList<ConversationSummaryResponse>>> GetConversationsAsync(Guid currentUserId);
}
