using Wedding_Proposal_BE.Features.Users.Application.Interfaces;
using Wedding_Proposal_BE.Features.Users.Infrastructure.Services;

namespace Wedding_Proposal_BE.Features.Users;

public static class UsersServiceRegistration
{
    public static IServiceCollection AddUsersFeature(this IServiceCollection services)
    {
        services.AddScoped<IUserService, UserService>();

        return services;
    }
}
