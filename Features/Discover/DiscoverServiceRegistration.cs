using Wedding_Proposal_BE.Features.Discover.Application.Interfaces;
using Wedding_Proposal_BE.Features.Discover.Infrastructure.Services;

namespace Wedding_Proposal_BE.Features.Discover;

public static class DiscoverServiceRegistration
{
    public static IServiceCollection AddDiscoverFeature(this IServiceCollection services)
    {
        services.AddScoped<IDiscoverService, DiscoverService>();
        return services;
    }
}
