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
        services.Configure<SmtpOptions>(configuration.GetSection(SmtpOptions.SectionName));

        var smtpOptions = configuration.GetSection(SmtpOptions.SectionName).Get<SmtpOptions>();
        if (string.IsNullOrWhiteSpace(smtpOptions?.Username) || string.IsNullOrWhiteSpace(smtpOptions.Password))
        {
            throw new InvalidOperationException("Smtp:Username and Smtp:Password must be configured.");
        }

        services.AddScoped<IEmailSender, SmtpEmailSender>();

        // No SMS/push providers are configured yet — logging stubs keep the app runnable end
        // to end. Swap these for real providers (e.g. Twilio, Firebase Cloud Messaging) here
        // once credentials exist.
        services.AddScoped<ISmsSender, LoggingSmsSender>();
        services.AddScoped<IPushSender, LoggingPushSender>();

        return services;
    }
}
