using Wedding_Proposal_BE.Features.Notification.EmailNotification.Application.Interfaces;
using Wedding_Proposal_BE.Features.Notification.EmailNotification.Infrastructure.Options;
using Wedding_Proposal_BE.Features.Notification.EmailNotification.Infrastructure.Services;
using Wedding_Proposal_BE.Features.Notification.PushNotification.Application.Interfaces;
using Wedding_Proposal_BE.Features.Notification.PushNotification.Infrastructure.Services;
using Wedding_Proposal_BE.Features.Notification.SmsNotification.Application.Interfaces;
using Wedding_Proposal_BE.Features.Notification.SmsNotification.Infrastructure.Services;

namespace Wedding_Proposal_BE.Features.Notification;

/// Registers everything the Notification feature owns: email, SMS, and push delivery. Other
/// features (e.g. Auth's OTP flows) depend only on IEmailSender/ISmsSender/IPushSender, never
/// on a concrete transport.
public static class NotificationServiceRegistration
{
    public static IServiceCollection AddNotificationFeature(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<SendPulseOptions>(configuration.GetSection(SendPulseOptions.SectionName));

        var sendPulseOptions = configuration.GetSection(SendPulseOptions.SectionName).Get<SendPulseOptions>();
        if (string.IsNullOrWhiteSpace(sendPulseOptions?.ClientId)
            || string.IsNullOrWhiteSpace(sendPulseOptions.ClientSecret)
            || string.IsNullOrWhiteSpace(sendPulseOptions.FromEmail))
        {
            throw new InvalidOperationException(
                "SendPulse:ClientId, SendPulse:ClientSecret and SendPulse:FromEmail must be configured.");
        }

        services.AddSingleton<SendPulseTokenCache>();
        services.AddHttpClient<IEmailSender, SendPulseEmailSender>(client =>
        {
            client.BaseAddress = new Uri(sendPulseOptions.ApiBaseUrl.TrimEnd('/') + "/");
            client.Timeout = TimeSpan.FromSeconds(30);
        });

        // No SMS/push providers are configured yet — logging stubs keep the app runnable end
        // to end. Swap these for real providers (e.g. Twilio, Firebase Cloud Messaging) here
        // once credentials exist.
        services.AddScoped<ISmsSender, LoggingSmsSender>();
        services.AddScoped<IPushSender, LoggingPushSender>();

        return services;
    }
}
