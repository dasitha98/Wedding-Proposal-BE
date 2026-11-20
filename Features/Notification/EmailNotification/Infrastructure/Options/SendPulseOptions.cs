namespace Wedding_Proposal_BE.Features.Notification.EmailNotification.Infrastructure.Options;

/// Credentials for the SendPulse REST API (Settings → API in the SendPulse dashboard).
/// FromEmail must be a sender address verified in your SendPulse SMTP account.
public class SendPulseOptions
{
    public const string SectionName = "SendPulse";

    public string ApiBaseUrl { get; set; } = "https://api.sendpulse.com";
    public string ClientId { get; set; } = string.Empty;
    public string ClientSecret { get; set; } = string.Empty;
    public string FromEmail { get; set; } = string.Empty;
    public string FromName { get; set; } = "Wedding Proposal";
}
