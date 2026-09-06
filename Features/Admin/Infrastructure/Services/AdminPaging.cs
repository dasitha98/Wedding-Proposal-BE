using Microsoft.EntityFrameworkCore;
using Wedding_Proposal_BE.Features.Admin.Application.DTOs;

namespace Wedding_Proposal_BE.Features.Admin.Infrastructure.Services;

/// Shared Skip/Take + count/map pipeline for admin list endpoints — mirrors the pagination
/// shape already used by Discover (DiscoverService), just factored out since Admin has many
/// entities that all need the same clamp/count/page/map steps.
internal static class AdminPaging
{
    public static async Task<AdminPagedResponse<TDto>> ToPagedAsync<TEntity, TDto>(
        IQueryable<TEntity> query, int page, int pageSize, Func<TEntity, TDto> map)
    {
        page = Math.Max(page, 1);
        pageSize = Math.Clamp(pageSize, 1, 100);

        var totalCount = await query.CountAsync();
        var totalPages = totalCount == 0 ? 0 : (int)Math.Ceiling(totalCount / (double)pageSize);

        var items = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();

        return new AdminPagedResponse<TDto>(items.Select(map).ToList(), totalCount, totalPages, page, pageSize);
    }
}
