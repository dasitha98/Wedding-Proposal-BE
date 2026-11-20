namespace Wedding_Proposal_BE.Features.Notification.EmailNotification.Application.Interfaces;

/// Generic email delivery abstraction. Any feature that needs to send an email (OTPs today,
/// future notifications) depends on this instead of a concrete transport.
public interface IEmailSender
{
    Task SendAsync(string toEmail, string subject, string body);
}
