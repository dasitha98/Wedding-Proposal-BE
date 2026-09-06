using Microsoft.EntityFrameworkCore;
using Wedding_Proposal_BE.Features.Admin.Application.DTOs;
using Wedding_Proposal_BE.Features.Admin.Application.Interfaces;
using Wedding_Proposal_BE.Features.Profiles.Domain.Enums;
using Wedding_Proposal_BE.Shared.Common;
using Wedding_Proposal_BE.Shared.Infrastructure.Data;

namespace Wedding_Proposal_BE.Features.Admin.Infrastructure.Services;

public class AdminEngagementService : IAdminEngagementService
{
    private readonly AppDbContext _dbContext;

    public AdminEngagementService(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Result<AdminPagedResponse<AdminConnectionRequestDto>>> ListConnectionRequestsAsync(int page, int pageSize)
    {
        var query = _dbContext.ProfileConnectionRequests.OrderByDescending(r => r.CreatedAt).AsQueryable();
        var paged = await AdminPaging.ToPagedAsync(query, page, pageSize, r => r);

        var userIds = paged.Items.SelectMany(r => new[] { r.RequesterUserId, r.TargetUserId }).Distinct().ToList();
        var names = await UserNamesAsync(userIds);

        var dtos = paged.Items.Select(r => new AdminConnectionRequestDto(
            r.Id, r.RequesterUserId, names.GetValueOrDefault(r.RequesterUserId, "Unknown"),
            r.TargetUserId, names.GetValueOrDefault(r.TargetUserId, "Unknown"),
            r.Status, r.DeclineCount, r.CreatedAt, r.UpdatedAt)).ToList();

        return Result.Success(new AdminPagedResponse<AdminConnectionRequestDto>(dtos, paged.TotalCount, paged.TotalPages, paged.Page, paged.PageSize));
    }

    public async Task<Result<AdminConnectionRequestDto>> UpdateConnectionRequestStatusAsync(Guid id, ConnectionStatus status)
    {
        var request = await _dbContext.ProfileConnectionRequests.FirstOrDefaultAsync(r => r.Id == id);
        if (request is null)
        {
            return Result.Failure<AdminConnectionRequestDto>("Connection request not found.", "REQUEST_NOT_FOUND");
        }

        switch (status)
        {
            case ConnectionStatus.Accepted:
                request.Accept();
                break;
            case ConnectionStatus.Declined:
                request.Decline();
                break;
            case ConnectionStatus.Sent:
                request.Resend();
                break;
        }

        await _dbContext.SaveChangesAsync();

        var names = await UserNamesAsync([request.RequesterUserId, request.TargetUserId]);
        return Result.Success(new AdminConnectionRequestDto(
            request.Id, request.RequesterUserId, names.GetValueOrDefault(request.RequesterUserId, "Unknown"),
            request.TargetUserId, names.GetValueOrDefault(request.TargetUserId, "Unknown"),
            request.Status, request.DeclineCount, request.CreatedAt, request.UpdatedAt));
    }

    public async Task<Result> DeleteConnectionRequestAsync(Guid id)
    {
        var request = await _dbContext.ProfileConnectionRequests.FirstOrDefaultAsync(r => r.Id == id);
        if (request is null)
        {
            return Result.Failure("Connection request not found.", "REQUEST_NOT_FOUND");
        }

        _dbContext.ProfileConnectionRequests.Remove(request);
        await _dbContext.SaveChangesAsync();
        return Result.Success();
    }

    public async Task<Result<AdminPagedResponse<AdminFavouriteDto>>> ListFavouritesAsync(int page, int pageSize)
    {
        var query = _dbContext.ProfileFavourites.OrderByDescending(f => f.CreatedAt).AsQueryable();
        var paged = await AdminPaging.ToPagedAsync(query, page, pageSize, f => f);

        var userIds = paged.Items.Select(f => f.UserId).Distinct().ToList();
        var names = await UserNamesAsync(userIds);

        var profileIds = paged.Items.Select(f => f.TargetProfileId).Distinct().ToList();
        var profileNames = await _dbContext.Profiles
            .Where(p => profileIds.Contains(p.Id))
            .ToDictionaryAsync(p => p.Id, p => $"{p.FirstName} {p.LastName}".Trim());

        var dtos = paged.Items.Select(f => new AdminFavouriteDto(
            f.Id, f.UserId, names.GetValueOrDefault(f.UserId, "Unknown"),
            f.TargetProfileId, profileNames.GetValueOrDefault(f.TargetProfileId, "Unknown"), f.CreatedAt)).ToList();

        return Result.Success(new AdminPagedResponse<AdminFavouriteDto>(dtos, paged.TotalCount, paged.TotalPages, paged.Page, paged.PageSize));
    }

    public async Task<Result> DeleteFavouriteAsync(Guid id)
    {
        var favourite = await _dbContext.ProfileFavourites.FirstOrDefaultAsync(f => f.Id == id);
        if (favourite is null)
        {
            return Result.Failure("Favourite not found.", "FAVOURITE_NOT_FOUND");
        }

        _dbContext.ProfileFavourites.Remove(favourite);
        await _dbContext.SaveChangesAsync();
        return Result.Success();
    }

    public async Task<Result<AdminPagedResponse<AdminConversationDto>>> ListConversationsAsync(int page, int pageSize)
    {
        var query = _dbContext.Conversations.OrderByDescending(c => c.CreatedAt).AsQueryable();
        var paged = await AdminPaging.ToPagedAsync(query, page, pageSize, c => c);

        var conversationIds = paged.Items.Select(c => c.Id).ToList();
        var messageCounts = await _dbContext.Messages
            .Where(m => conversationIds.Contains(m.ConversationId))
            .GroupBy(m => m.ConversationId)
            .Select(g => new { ConversationId = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.ConversationId, x => x.Count);

        var userIds = paged.Items.SelectMany(c => new[] { c.UserAId, c.UserBId }).Distinct().ToList();
        var names = await UserNamesAsync(userIds);

        var dtos = paged.Items.Select(c => new AdminConversationDto(
            c.Id, c.UserAId, names.GetValueOrDefault(c.UserAId, "Unknown"),
            c.UserBId, names.GetValueOrDefault(c.UserBId, "Unknown"),
            c.CreatedAt, messageCounts.GetValueOrDefault(c.Id, 0))).ToList();

        return Result.Success(new AdminPagedResponse<AdminConversationDto>(dtos, paged.TotalCount, paged.TotalPages, paged.Page, paged.PageSize));
    }

    public async Task<Result> DeleteConversationAsync(Guid id)
    {
        var conversation = await _dbContext.Conversations.FirstOrDefaultAsync(c => c.Id == id);
        if (conversation is null)
        {
            return Result.Failure("Conversation not found.", "CONVERSATION_NOT_FOUND");
        }

        _dbContext.Conversations.Remove(conversation);
        await _dbContext.SaveChangesAsync();
        return Result.Success();
    }

    public async Task<Result<AdminPagedResponse<AdminMessageDto>>> ListMessagesAsync(Guid conversationId, int page, int pageSize)
    {
        var query = _dbContext.Messages.Where(m => m.ConversationId == conversationId).OrderByDescending(m => m.SentAt).AsQueryable();
        var paged = await AdminPaging.ToPagedAsync(query, page, pageSize, m => m);

        var userIds = paged.Items.Select(m => m.SenderUserId).Distinct().ToList();
        var names = await UserNamesAsync(userIds);

        var dtos = paged.Items.Select(m => new AdminMessageDto(
            m.Id, m.ConversationId, m.SenderUserId, names.GetValueOrDefault(m.SenderUserId, "Unknown"), m.Text, m.SentAt)).ToList();

        return Result.Success(new AdminPagedResponse<AdminMessageDto>(dtos, paged.TotalCount, paged.TotalPages, paged.Page, paged.PageSize));
    }

    public async Task<Result> DeleteMessageAsync(Guid id)
    {
        var message = await _dbContext.Messages.FirstOrDefaultAsync(m => m.Id == id);
        if (message is null)
        {
            return Result.Failure("Message not found.", "MESSAGE_NOT_FOUND");
        }

        _dbContext.Messages.Remove(message);
        await _dbContext.SaveChangesAsync();
        return Result.Success();
    }

    private async Task<Dictionary<Guid, string>> UserNamesAsync(List<Guid> userIds) =>
        await _dbContext.Users
            .Where(u => userIds.Contains(u.Id))
            .ToDictionaryAsync(u => u.Id, u => $"{u.FirstName} {u.LastName}".Trim());
}
