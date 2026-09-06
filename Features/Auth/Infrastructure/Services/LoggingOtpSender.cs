using Microsoft.Extensions.Logging;

namespace Wedding_Proposal_BE.Features.Auth.Infrastructure.Services;

/// Dev-only IOtpSender: logs the code instead of emailing/texting it, since no SMTP/SMS
/// provider is configured in this environment. Replace with a real sender before production —
/// this is the one intentional stub in the auth flow, and it is scoped to just this class.
/// Shared by both the password-reset and registration-verification OTP flows.
public class LoggingOtpSender : IOtpSender
{
    private readonly ILogger<LoggingOtpSender> _logger;

    public LoggingOtpSender(ILogger<LoggingOtpSender> logger)
    {
        _logger = logger;
    }

    public Task SendAsync(string email, string code)
    {
        _logger.LogInformation("OTP for {Email}: {Code}", email, code);
        return Task.CompletedTask;
    }
}
