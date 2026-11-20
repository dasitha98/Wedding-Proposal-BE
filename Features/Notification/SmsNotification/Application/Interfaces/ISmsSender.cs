namespace Wedding_Proposal_BE.Features.Notification.SmsNotification.Application.Interfaces;

/// SMS delivery abstraction. No real provider (e.g. Twilio) is wired up yet — see
/// LoggingSmsSender and NotificationServiceRegistration.
public interface ISmsSender
{
    Task SendAsync(string phoneNumber, string message);
}
