namespace Wedding_Proposal_BE.Features.Notification.PushNotification.Application.Interfaces;

/// Push notification delivery abstraction. No real provider (e.g. Firebase Cloud Messaging,
/// Expo push) is wired up yet — see LoggingPushSender and NotificationServiceRegistration.
public interface IPushSender
{
    Task SendAsync(string deviceToken, string title, string body);
}
