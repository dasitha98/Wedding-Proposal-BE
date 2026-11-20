using Wedding_Proposal_BE.Features.Auth.Application.Interfaces;
using Wedding_Proposal_BE.Features.Notification.EmailNotification.Application.Interfaces;

namespace Wedding_Proposal_BE.Features.Auth.Infrastructure.Services;

/// Formats OTP codes as an email and delegates delivery to the Notification feature's
/// IEmailSender (SendPulse, as configured in
/// NotificationServiceRegistration).
public class EmailOtpSender : IOtpSender
{
    private readonly IEmailSender _emailSender;

    public EmailOtpSender(IEmailSender emailSender)
    {
        _emailSender = emailSender;
    }

    public Task SendAsync(string email, string code)
    {
        var body = $"Your one-time verification code is: {code}\n\nThis code expires in 5 minutes. If you didn't request this, you can ignore this email.";
        return _emailSender.SendAsync(email, "Your verification code", body);
    }
}
