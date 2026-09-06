using Wedding_Proposal_BE.Features.Profiles.Application.Interfaces;
using Wedding_Proposal_BE.Features.Profiles.Infrastructure.Services;

namespace Wedding_Proposal_BE.Features.Profiles;

public static class ProfilesServiceRegistration
{
    public static IServiceCollection AddProfilesFeature(this IServiceCollection services)
    {
        services.AddScoped<IProfileService, ProfileService>();

        return services;
    }
}
