using Microsoft.Extensions.Logging;
using Wedding_Proposal_BE.Features.Notification.SmsNotification.Application.Interfaces;

namespace Wedding_Proposal_BE.Features.Notification.SmsNotification.Infrastructure.Services;

/// Stub ISmsSender: logs the message instead of texting it, since no SMS provider (e.g.
/// Twilio) is configured yet. Swap for a real implementation in
/// NotificationServiceRegistration once one is added.
public class LoggingSmsSender : ISmsSender
{
    private readonly ILogger<LoggingSmsSender> _logger;

    public LoggingSmsSender(ILogger<LoggingSmsSender> logger)
    {
        _logger = logger;
    }

    public Task SendAsync(string phoneNumber, string message)
    {
        _logger.LogInformation("SMS to {PhoneNumber}: {Message}", phoneNumber, message);
        return Task.CompletedTask;
    }
}
