using Wedding_Proposal_BE.Features.Messages.Application.Interfaces;
using Wedding_Proposal_BE.Features.Messages.Infrastructure.Services;

namespace Wedding_Proposal_BE.Features.Messages;

public static class MessagesServiceRegistration
{
    public static IServiceCollection AddMessagesFeature(this IServiceCollection services)
    {
        services.AddScoped<IMessageService, MessageService>();
        return services;
    }
}
