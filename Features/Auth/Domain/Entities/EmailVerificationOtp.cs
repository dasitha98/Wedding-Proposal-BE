namespace Wedding_Proposal_BE.Features.Auth.Domain.Entities;

/// One row per outstanding registration-verification OTP for an email. A new request replaces
/// the previous row for that email (see AuthService) rather than accumulating history.
public class EmailVerificationOtp
{
    private const int MaxAttempts = 5;

    public Guid Id { get; private set; }
    public string Email { get; private set; } = string.Empty;
    public string CodeHash { get; private set; } = string.Empty;
    public DateTime ExpiresAt { get; private set; }
    public DateTime ResendAvailableAt { get; private set; }
    public int AttemptsRemaining { get; private set; }
    public DateTime? ConsumedAt { get; private set; }

    public bool IsExpired => DateTime.UtcNow >= ExpiresAt;
    public bool IsConsumed => ConsumedAt is not null;

    private EmailVerificationOtp()
    {
    }

    public static EmailVerificationOtp Issue(string email, string codeHash, TimeSpan validFor, TimeSpan resendCooldown)
    {
        var now = DateTime.UtcNow;
        return new EmailVerificationOtp
        {
            Id = Guid.NewGuid(),
            Email = email,
            CodeHash = codeHash,
            ExpiresAt = now.Add(validFor),
            ResendAvailableAt = now.Add(resendCooldown),
            AttemptsRemaining = MaxAttempts
        };
    }

    /// Returns true when codeHash matches; otherwise decrements the remaining attempts.
    public bool TryConsume(string codeHash)
    {
        if (IsConsumed || IsExpired || AttemptsRemaining <= 0)
        {
            return false;
        }

        if (CodeHash != codeHash)
        {
            AttemptsRemaining -= 1;
            return false;
        }

        ConsumedAt = DateTime.UtcNow;
        return true;
    }
}
