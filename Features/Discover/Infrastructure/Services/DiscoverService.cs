using Microsoft.EntityFrameworkCore;
using Wedding_Proposal_BE.Features.Discover.Application.DTOs;
using Wedding_Proposal_BE.Features.Discover.Application.Interfaces;
using Wedding_Proposal_BE.Shared.Common;
using Wedding_Proposal_BE.Shared.Infrastructure.Data;

namespace Wedding_Proposal_BE.Features.Discover.Infrastructure.Services;

/// Read-only projection over Profiles — no entities of its own. Every rule here mirrors
/// discover/domain/usecases/filterProfiles.ts and sortProfiles.ts on the FE, translated into
/// an EF LINQ query so filtering/sorting/pagination all happen in the database.
public class DiscoverService : IDiscoverService
{
    private const string SriLanka = "Sri Lanka";

    private readonly AppDbContext _dbContext;

    public DiscoverService(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Result<DiscoverResultResponse>> GetProfilesAsync(Guid? viewerUserId, DiscoverFiltersRequest filters)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var maxDateOfBirth = today.AddYears(-filters.MinAge);
        var minDateOfBirth = today.AddYears(-(filters.MaxAge + 1)).AddDays(1);

        var query = _dbContext.Profiles
            .Include(p => p.Photos)
            .Where(p => viewerUserId == null || p.UserId != viewerUserId)
            .Where(p => p.DateOfBirth >= minDateOfBirth && p.DateOfBirth <= maxDateOfBirth)
            .AsQueryable();

        if (filters.Genders.Count > 0)
        {
            query = query.Where(p => filters.Genders.Contains(p.BasicInfo.Gender));
        }

        if (!string.IsNullOrWhiteSpace(filters.Country))
        {
            query = query.Where(p => p.Residency.Country == filters.Country);
        }

        if (filters.Country == SriLanka)
        {
            if (!string.IsNullOrWhiteSpace(filters.District))
            {
                query = query.Where(p => p.Residency.District == filters.District);

                if (!string.IsNullOrWhiteSpace(filters.City))
                {
                    query = query.Where(p => p.Residency.City == filters.City);
                }
            }
        }

        if (filters.Religions.Count > 0)
        {
            query = query.Where(p => filters.Religions.Contains(p.BasicInfo.Religion));
        }

        if (filters.Races.Count > 0)
        {
            query = query.Where(p => filters.Races.Contains(p.BasicInfo.Race));
        }

        if (filters.Castes.Count > 0)
        {
            query = query.Where(p => filters.Castes.Contains(p.BasicInfo.Caste));
        }

        if (filters.MaritalStatuses.Count > 0)
        {
            query = query.Where(p => filters.MaritalStatuses.Contains(p.BasicInfo.MaritalStatus));
        }

        if (!string.IsNullOrWhiteSpace(filters.JobTitle))
        {
            // Plain ToLower/Contains rather than EF.Functions.ILike so this also works against
            // the InMemory provider used when no DB connection string is configured yet.
            var jobTitleLower = filters.JobTitle.ToLower();
            query = query.Where(p => p.Profession.Occupation.ToLower().Contains(jobTitleLower));
        }

        if (filters.OnlyForeign)
        {
            query = query.Where(p => p.Residency.Country != SriLanka);
        }

        if (filters.OnlyGold)
        {
            query = query.Where(p => p.IsGold);
        }

        if (filters.OnlyWithPhotos)
        {
            query = query.Where(p => p.Photos.Any());
        }

        query = filters.SortBy == "oldest"
            ? query.OrderBy(p => p.CreatedAt)
            : query.OrderByDescending(p => p.CreatedAt);

        var totalCount = await query.CountAsync();
        var pageSize = filters.PageSize <= 0 ? 14 : filters.PageSize;
        var page = filters.Page <= 0 ? 1 : filters.Page;
        var totalPages = Math.Max(1, (int)Math.Ceiling(totalCount / (double)pageSize));

        var profiles = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var items = profiles.Select(p => new DiscoverProfileResponse(
            p.Id,
            $"{p.FirstName} {p.LastName}".Trim(),
            p.CalculateAge(today),
            p.BasicInfo.Gender,
            p.AboutMe,
            p.Residency.Country,
            p.Residency.Country == SriLanka ? p.Residency.District : null,
            p.Residency.Country == SriLanka ? p.Residency.City : null,
            p.BasicInfo.Religion,
            p.BasicInfo.Race,
            string.IsNullOrEmpty(p.BasicInfo.Caste) ? null : p.BasicInfo.Caste,
            p.BasicInfo.MaritalStatus,
            p.Profession.Occupation,
            p.CreatedAt,
            p.IsGold,
            p.Photos.OrderBy(ph => ph.Order).Select(ph => ph.Uri).ToList(),
            p.Hobbies,
            p.Verification.IdentityVerified))
            .ToList();

        return Result.Success(new DiscoverResultResponse(items, totalCount, totalPages, page, pageSize));
    }
}
