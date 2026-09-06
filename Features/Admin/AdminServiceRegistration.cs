using Wedding_Proposal_BE.Features.Admin.Application.Interfaces;
using Wedding_Proposal_BE.Features.Admin.Infrastructure.Services;

namespace Wedding_Proposal_BE.Features.Admin;

public static class AdminServiceRegistration
{
    public static IServiceCollection AddAdminFeature(this IServiceCollection services)
    {
        services.AddScoped<IAdminUserService, AdminUserService>();
        services.AddScoped<IAdminRoleService, AdminRoleService>();
        services.AddScoped<IAdminProfileService, AdminProfileService>();
        services.AddScoped<IAdminEngagementService, AdminEngagementService>();
        services.AddScoped<IAdminSecurityService, AdminSecurityService>();

        return services;
    }
}
