using Wedding_Proposal_BE.Features.Admin.Application.DTOs;
using Wedding_Proposal_BE.Shared.Common;

namespace Wedding_Proposal_BE.Features.Admin.Application.Interfaces;

public interface IAdminRoleService
{
    Task<Result<IReadOnlyList<AdminRoleDto>>> ListAsync();
}
