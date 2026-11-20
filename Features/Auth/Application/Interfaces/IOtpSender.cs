namespace Wedding_Proposal_BE.Features.Auth.Application.Interfaces;

/// Delivery abstraction for one-time codes (password-reset and registration-verification).
/// Callers depend only on this interface, never on a concrete transport — see
/// EmailOtpSender, which delegates to the Notification feature's IEmailSender.
public interface IOtpSender
{
    Task SendAsync(string email, string code);
}
