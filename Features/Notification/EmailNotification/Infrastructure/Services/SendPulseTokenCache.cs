namespace Wedding_Proposal_BE.Features.Notification.EmailNotification.Infrastructure.Services;

/// Singleton holder for the SendPulse OAuth access token so it is reused across requests
/// (tokens live for 1 hour) instead of fetching a new one for every email.
public class SendPulseTokenCache
{
    public SemaphoreSlim Lock { get; } = new(1, 1);
    public string? AccessToken { get; set; }
    public DateTimeOffset ExpiresAt { get; set; }

    public bool IsValid => AccessToken is not null && DateTimeOffset.UtcNow < ExpiresAt;

    public void Invalidate() => AccessToken = null;
}
