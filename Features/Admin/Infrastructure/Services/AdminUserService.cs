using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Wedding_Proposal_BE.Features.Admin.Application.DTOs;
using Wedding_Proposal_BE.Features.Admin.Application.Interfaces;
using Wedding_Proposal_BE.Shared.Common;
using Wedding_Proposal_BE.Shared.Infrastructure.Data;
using Wedding_Proposal_BE.Shared.Infrastructure.Identity;

namespace Wedding_Proposal_BE.Features.Admin.Infrastructure.Services;

public class AdminUserService : IAdminUserService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly AppDbContext _dbContext;

    public AdminUserService(UserManager<ApplicationUser> userManager, AppDbContext dbContext)
    {
        _userManager = userManager;
        _dbContext = dbContext;
    }

    public async Task<Result<AdminPagedResponse<AdminUserDto>>> ListAsync(string? search, int page, int pageSize)
    {
        var query = _userManager.Users.AsQueryable();
        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim().ToLower();
            query = query.Where(u =>
                u.FirstName.ToLower().Contains(term) ||
                u.LastName.ToLower().Contains(term) ||
                (u.Email ?? string.Empty).ToLower().Contains(term));
        }
        query = query.OrderByDescending(u => u.CreatedAt);

        var paged = await AdminPaging.ToPagedAsync(query, page, pageSize, u => u);
        var dtos = await ToDtosAsync(paged.Items);

        return Result.Success(new AdminPagedResponse<AdminUserDto>(dtos, paged.TotalCount, paged.TotalPages, paged.Page, paged.PageSize));
    }

    public async Task<Result<AdminUserDto>> CreateAsync(AdminCreateUserRequest request)
    {
        var normalizedEmail = request.Email.Trim().ToLowerInvariant();
        var existing = await _userManager.FindByEmailAsync(normalizedEmail);
        if (existing is not null)
        {
            return Result.Failure<AdminUserDto>("An account with that email already exists.", "EMAIL_IN_USE");
        }

        var user = ApplicationUser.Create(normalizedEmail, request.FirstName.Trim(), request.LastName.Trim());
        user.EmailConfirmed = true;
        var createResult = await _userManager.CreateAsync(user, request.Password);
        if (!createResult.Succeeded)
        {
            var message = string.Join(" ", createResult.Errors.Select(e => e.Description));
            return Result.Failure<AdminUserDto>(message, "CREATE_FAILED");
        }

        var roles = (request.Roles ?? []).Append(RoleNames.User).Distinct().ToList();
        await _userManager.AddToRolesAsync(user, roles);

        return Result.Success(await ToDtoAsync(user));
    }

    public async Task<Result<AdminUserDto>> GetByIdAsync(Guid id)
    {
        var user = await _userManager.FindByIdAsync(id.ToString());
        if (user is null)
        {
            return Result.Failure<AdminUserDto>("User not found.", "USER_NOT_FOUND");
        }

        return Result.Success(await ToDtoAsync(user));
    }

    public async Task<Result<AdminUserDto>> UpdateAsync(Guid id, AdminUpdateUserRequest request)
    {
        var user = await _userManager.FindByIdAsync(id.ToString());
        if (user is null)
        {
            return Result.Failure<AdminUserDto>("User not found.", "USER_NOT_FOUND");
        }

        if (request.FirstName is not null || request.LastName is not null)
        {
            user.UpdateName(request.FirstName ?? user.FirstName, request.LastName ?? user.LastName);
        }

        if (!string.IsNullOrWhiteSpace(request.Email) && !string.Equals(request.Email, user.Email, StringComparison.OrdinalIgnoreCase))
        {
            var normalizedEmail = request.Email.Trim().ToLowerInvariant();
            var owner = await _userManager.FindByEmailAsync(normalizedEmail);
            if (owner is not null && owner.Id != user.Id)
            {
                return Result.Failure<AdminUserDto>("An account with that email already exists.", "EMAIL_IN_USE");
            }

            await _userManager.SetEmailAsync(user, normalizedEmail);
            await _userManager.SetUserNameAsync(user, normalizedEmail);
        }

        if (request.EmailConfirmed.HasValue)
        {
            user.EmailConfirmed = request.EmailConfirmed.Value;
        }

        await _userManager.UpdateAsync(user);

        if (request.IsLockedOut.HasValue)
        {
            if (request.IsLockedOut.Value)
            {
                user.LockoutEnabled = true;
                await _userManager.UpdateAsync(user);
                await _userManager.SetLockoutEndDateAsync(user, DateTimeOffset.MaxValue);
            }
            else
            {
                await _userManager.SetLockoutEndDateAsync(user, null);
            }
        }

        if (request.Roles is not null)
        {
            var currentRoles = await _userManager.GetRolesAsync(user);
            var rolesToRemove = currentRoles.Except(request.Roles, StringComparer.OrdinalIgnoreCase).ToList();
            var rolesToAdd = request.Roles.Except(currentRoles, StringComparer.OrdinalIgnoreCase).ToList();
            if (rolesToRemove.Count > 0) await _userManager.RemoveFromRolesAsync(user, rolesToRemove);
            if (rolesToAdd.Count > 0) await _userManager.AddToRolesAsync(user, rolesToAdd);
        }

        return Result.Success(await ToDtoAsync(user));
    }

    public async Task<Result> DeleteAsync(Guid id)
    {
        var user = await _userManager.FindByIdAsync(id.ToString());
        if (user is null)
        {
            return Result.Failure("User not found.", "USER_NOT_FOUND");
        }

        try
        {
            var result = await _userManager.DeleteAsync(user);
            if (!result.Succeeded)
            {
                var message = string.Join(" ", result.Errors.Select(e => e.Description));
                return Result.Failure(message, "DELETE_FAILED");
            }
        }
        catch (DbUpdateException)
        {
            return Result.Failure(
                "Cannot delete this user because related data exists (e.g. a profile). Delete their profile first.", "DELETE_BLOCKED");
        }

        return Result.Success();
    }

    private async Task<AdminUserDto> ToDtoAsync(ApplicationUser user)
    {
        var roles = await _userManager.GetRolesAsync(user);
        var isLockedOut = await _userManager.IsLockedOutAsync(user);
        var profileId = await _dbContext.Profiles.Where(p => p.UserId == user.Id).Select(p => (Guid?)p.Id).FirstOrDefaultAsync();
        return new AdminUserDto(user.Id, user.FirstName, user.LastName, user.Email ?? string.Empty, user.EmailConfirmed, isLockedOut, user.CreatedAt, user.LastActiveAt, roles.ToList(), profileId);
    }

    private async Task<List<AdminUserDto>> ToDtosAsync(IReadOnlyList<ApplicationUser> users)
    {
        var userIds = users.Select(u => u.Id).ToList();
        var now = DateTimeOffset.UtcNow;

        var rolesByUser = await (
            from ur in _dbContext.UserRoles
            join r in _dbContext.Roles on ur.RoleId equals r.Id
            where userIds.Contains(ur.UserId)
            select new { ur.UserId, r.Name }
        ).ToListAsync();

        var rolesLookup = rolesByUser
            .GroupBy(x => x.UserId)
            .ToDictionary(g => g.Key, g => (IReadOnlyList<string>)g.Select(x => x.Name ?? string.Empty).ToList());

        var profileLookup = await _dbContext.Profiles
            .Where(p => userIds.Contains(p.UserId))
            .ToDictionaryAsync(p => p.UserId, p => p.Id);

        return users.Select(u => new AdminUserDto(
            u.Id, u.FirstName, u.LastName, u.Email ?? string.Empty, u.EmailConfirmed,
            u.LockoutEnabled && u.LockoutEnd.HasValue && u.LockoutEnd > now,
            u.CreatedAt,
            u.LastActiveAt,
            rolesLookup.TryGetValue(u.Id, out var roles) ? roles : [],
            profileLookup.TryGetValue(u.Id, out var profileId) ? profileId : null)).ToList();
    }
}
