namespace Wedding_Proposal_BE.Features.Auth.Infrastructure.Services;

/// Delivery abstraction for one-time codes (password-reset and registration-verification).
/// Swap the registered implementation (see AuthServiceRegistration) for a real email/SMS
/// provider in production — AuthService only depends on this interface, never on a concrete
/// transport.
public interface IOtpSender
{
    Task SendAsync(string email, string code);
}
