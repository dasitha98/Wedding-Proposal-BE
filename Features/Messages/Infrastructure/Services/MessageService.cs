using Microsoft.EntityFrameworkCore;
using Wedding_Proposal_BE.Features.Messages.Application.DTOs;
using Wedding_Proposal_BE.Features.Messages.Application.Interfaces;
using Wedding_Proposal_BE.Features.Messages.Domain.Entities;
using Wedding_Proposal_BE.Features.Profiles.Application.DTOs;
using Wedding_Proposal_BE.Features.Profiles.Domain.Enums;
using Wedding_Proposal_BE.Shared.Common;
using Wedding_Proposal_BE.Shared.Infrastructure.Data;
using Profile = Wedding_Proposal_BE.Features.Profiles.Domain.Entities.Profile;

namespace Wedding_Proposal_BE.Features.Messages.Infrastructure.Services;

/// Reads Profiles/ProfileConnectionRequests directly off the shared AppDbContext (no
/// cross-feature service dependency) to resolve the profileId route param to a user and to
/// enforce the connect-before-chat rule the FE only enforces cosmetically today.
public class MessageService : IMessageService
{
    private readonly AppDbContext _dbContext;

    public MessageService(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Result<IReadOnlyList<MessageResponse>>> GetConversationAsync(Guid currentUserId, Guid profileId)
    {
        var resolved = await ResolveConversationAsync(currentUserId, profileId, createIfMissing: false);
        if (resolved.IsFailure)
        {
            return Result.Failure<IReadOnlyList<MessageResponse>>(resolved.Error!, resolved.ErrorCode);
        }

        if (resolved.Value is null)
        {
            // No conversation yet (nobody has sent a first message) — an empty thread, not an error.
            return Result.Success<IReadOnlyList<MessageResponse>>([]);
        }

        var messages = await _dbContext.Messages
            .Where(m => m.ConversationId == resolved.Value.Id)
            .OrderBy(m => m.SentAt)
            .Select(m => new MessageResponse(m.Id, m.Text, m.SenderUserId, m.SentAt))
            .ToListAsync();

        resolved.Value.MarkReadFor(currentUserId);
        await _dbContext.SaveChangesAsync();

        return Result.Success<IReadOnlyList<MessageResponse>>(messages);
    }

    public async Task<Result<IReadOnlyList<ConversationSummaryResponse>>> GetConversationsAsync(Guid currentUserId)
    {
        var conversations = await _dbContext.Conversations
            .Where(c => c.UserAId == currentUserId || c.UserBId == currentUserId)
            .ToListAsync();

        var summaries = new List<ConversationSummaryResponse>();
        foreach (var conversation in conversations)
        {
            var otherUserId = conversation.UserAId == currentUserId ? conversation.UserBId : conversation.UserAId;

            var lastMessage = await _dbContext.Messages
                .Where(m => m.ConversationId == conversation.Id)
                .OrderByDescending(m => m.SentAt)
                .FirstOrDefaultAsync();
            if (lastMessage is null) continue;

            var otherProfile = await _dbContext.Profiles
                .Include(p => p.Photos)
                .FirstOrDefaultAsync(p => p.UserId == otherUserId);
            if (otherProfile is null) continue;

            var lastReadAt = conversation.LastReadAtFor(currentUserId);
            var unreadCount = await _dbContext.Messages.CountAsync(m =>
                m.ConversationId == conversation.Id &&
                m.SenderUserId != currentUserId &&
                (lastReadAt == null || m.SentAt > lastReadAt));

            summaries.Add(new ConversationSummaryResponse(
                otherProfile.Id,
                new ProfileSummaryResponse(
                    otherProfile.Id,
                    otherProfile.FirstName,
                    otherProfile.LastName,
                    otherProfile.Photos.OrderBy(p => p.Order).Select(p => p.Uri).FirstOrDefault(),
                    otherProfile.CalculateAge(),
                    otherProfile.Residency.City,
                    FormatLastActiveLabel(otherProfile.UpdatedAt),
                    otherProfile.IsGold),
                lastMessage.Text,
                lastMessage.SentAt,
                lastMessage.SenderUserId == currentUserId,
                unreadCount));
        }

        return Result.Success<IReadOnlyList<ConversationSummaryResponse>>(
            summaries.OrderByDescending(s => s.LastMessageAt).ToList());
    }

    private static string FormatLastActiveLabel(DateTime lastActiveAt)
    {
        var elapsed = DateTime.UtcNow - lastActiveAt;
        if (elapsed <= TimeSpan.FromMinutes(5)) return "Online now";
        if (elapsed < TimeSpan.FromHours(1)) return $"Active {(int)elapsed.TotalMinutes}m ago";
        if (elapsed < TimeSpan.FromDays(1)) return $"Active {(int)elapsed.TotalHours}h ago";
        return $"Active {(int)elapsed.TotalDays}d ago";
    }

    public async Task<Result<MessageResponse>> SendMessageAsync(Guid currentUserId, Guid profileId, string text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return Result.Failure<MessageResponse>("Message text cannot be empty.", "EMPTY_MESSAGE");
        }

        var resolved = await ResolveConversationAsync(currentUserId, profileId, createIfMissing: true);
        if (resolved.IsFailure)
        {
            return Result.Failure<MessageResponse>(resolved.Error!, resolved.ErrorCode);
        }

        var conversation = resolved.Value!;
        var message = Message.Create(conversation.Id, currentUserId, text.Trim());
        _dbContext.Messages.Add(message);
        await _dbContext.SaveChangesAsync();

        return Result.Success(new MessageResponse(message.Id, message.Text, message.SenderUserId, message.SentAt));
    }

    private async Task<Result<Conversation?>> ResolveConversationAsync(Guid currentUserId, Guid profileId, bool createIfMissing)
    {
        var otherProfile = await _dbContext.Profiles.FirstOrDefaultAsync(p => p.Id == profileId);
        if (otherProfile is null)
        {
            return Result.Failure<Conversation?>("This profile could not be found.", "PROFILE_NOT_FOUND");
        }

        var otherUserId = otherProfile.UserId;
        var (userAId, userBId) = currentUserId.CompareTo(otherUserId) <= 0
            ? (currentUserId, otherUserId)
            : (otherUserId, currentUserId);

        var existing = await _dbContext.Conversations
            .FirstOrDefaultAsync(c => c.UserAId == userAId && c.UserBId == userBId);
        if (existing is not null)
        {
            return Result.Success<Conversation?>(existing);
        }

        if (!createIfMissing)
        {
            return Result.Success<Conversation?>(null);
        }

        var isConnected = await _dbContext.ProfileConnectionRequests.AnyAsync(r =>
            r.Status == ConnectionStatus.Accepted &&
            ((r.RequesterUserId == currentUserId && r.TargetUserId == otherUserId) ||
             (r.RequesterUserId == otherUserId && r.TargetUserId == currentUserId)));

        if (!isConnected)
        {
            return Result.Failure<Conversation?>("You can only message profiles you're connected with.", "NOT_CONNECTED");
        }

        var conversation = Conversation.Create(currentUserId, otherUserId);
        _dbContext.Conversations.Add(conversation);
        await _dbContext.SaveChangesAsync();

        return Result.Success<Conversation?>(conversation);
    }
}
