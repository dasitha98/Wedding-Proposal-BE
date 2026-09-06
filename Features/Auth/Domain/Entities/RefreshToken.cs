namespace Wedding_Proposal_BE.Features.Auth.Domain.Entities;

/// Encapsulates refresh-token lifecycle behavior (rotation/revocation) instead of exposing
/// public setters that would let callers mutate state without going through invariants.
public class RefreshToken
{
    public Guid Id { get; private set; }
    public Guid UserId { get; private set; }
    public string Token { get; private set; } = string.Empty;
    public DateTime ExpiresAt { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? RevokedAt { get; private set; }
    public string? ReplacedByToken { get; private set; }

    public bool IsExpired => DateTime.UtcNow >= ExpiresAt;
    public bool IsRevoked => RevokedAt is not null;
    public bool IsActive => !IsExpired && !IsRevoked;

    private RefreshToken()
    {
    }

    public static RefreshToken Issue(Guid userId, string token, TimeSpan lifetime)
    {
        return new RefreshToken
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Token = token,
            CreatedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.Add(lifetime)
        };
    }

    public void Revoke(string? replacedByToken = null)
    {
        if (IsRevoked)
        {
            return;
        }

        RevokedAt = DateTime.UtcNow;
        ReplacedByToken = replacedByToken;
    }
}
