using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MimeKit;
using Wedding_Proposal_BE.Features.Auth.Infrastructure.Options;

namespace Wedding_Proposal_BE.Features.Auth.Infrastructure.Services;

/// Sends OTP codes over real SMTP (e.g. Gmail with an app password). Registered instead of
/// LoggingOtpSender once Smtp:Username/Password are configured — see AuthServiceRegistration.
public class SmtpOtpSender : IOtpSender
{
    private readonly SmtpOptions _options;
    private readonly ILogger<SmtpOtpSender> _logger;

    public SmtpOtpSender(IOptions<SmtpOptions> options, ILogger<SmtpOtpSender> logger)
    {
        _options = options.Value;
        _logger = logger;
    }

    public async Task SendAsync(string email, string code)
    {
        var message = new MimeMessage();
        message.From.Add(new MailboxAddress(_options.FromName, _options.FromEmail));
        message.To.Add(MailboxAddress.Parse(email));
        message.Subject = "Your verification code";
        message.Body = new TextPart("plain")
        {
            Text = $"Your one-time verification code is: {code}\n\nThis code expires in 5 minutes. If you didn't request this, you can ignore this email."
        };

        using var client = new SmtpClient();
        try
        {
            await client.ConnectAsync(_options.Host, _options.Port, SecureSocketOptions.StartTls);
            await client.AuthenticateAsync(_options.Username, _options.Password);
            await client.SendAsync(message);
        }
        finally
        {
            if (client.IsConnected)
            {
                await client.DisconnectAsync(true);
            }
        }

        _logger.LogInformation("OTP email sent to {Email}", email);
    }
}
