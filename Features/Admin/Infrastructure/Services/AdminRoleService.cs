using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Wedding_Proposal_BE.Features.Admin.Application.DTOs;
using Wedding_Proposal_BE.Features.Admin.Application.Interfaces;
using Wedding_Proposal_BE.Shared.Common;

namespace Wedding_Proposal_BE.Features.Admin.Infrastructure.Services;

public class AdminRoleService : IAdminRoleService
{
    private readonly RoleManager<IdentityRole<Guid>> _roleManager;

    public AdminRoleService(RoleManager<IdentityRole<Guid>> roleManager)
    {
        _roleManager = roleManager;
    }

    public async Task<Result<IReadOnlyList<AdminRoleDto>>> ListAsync()
    {
        var roles = await _roleManager.Roles.OrderBy(r => r.Name).ToListAsync();
        return Result.Success<IReadOnlyList<AdminRoleDto>>(
            roles.Select(r => new AdminRoleDto(r.Id, r.Name ?? string.Empty)).ToList());
    }
}
