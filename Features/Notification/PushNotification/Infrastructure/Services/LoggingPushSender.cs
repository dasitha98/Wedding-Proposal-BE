using Microsoft.Extensions.Logging;
using Wedding_Proposal_BE.Features.Notification.PushNotification.Application.Interfaces;

namespace Wedding_Proposal_BE.Features.Notification.PushNotification.Infrastructure.Services;

/// Stub IPushSender: logs the notification instead of pushing it, since no push provider
/// (e.g. FCM, Expo push) is configured yet. Swap for a real implementation in
/// NotificationServiceRegistration once one is added.
public class LoggingPushSender : IPushSender
{
    private readonly ILogger<LoggingPushSender> _logger;

    public LoggingPushSender(ILogger<LoggingPushSender> logger)
    {
        _logger = logger;
    }

    public Task SendAsync(string deviceToken, string title, string body)
    {
        _logger.LogInformation("Push to {DeviceToken} | {Title}: {Body}", deviceToken, title, body);
        return Task.CompletedTask;
    }
}
