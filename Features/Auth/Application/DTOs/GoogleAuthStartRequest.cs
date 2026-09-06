using System.ComponentModel.DataAnnotations;

namespace Wedding_Proposal_BE.Features.Auth.Application.DTOs;

/// The URI Google should redirect back to once the user has authenticated — the app's own
/// deep link (e.g. `weddingproposal://auth/google`, or an `exp://…` dev-client URI). Must
/// match one of `Google:AllowedRedirectUris`; anything else is rejected.
public class GoogleAuthStartRequest
{
    [Required]
    public string RedirectUri { get; set; } = string.Empty;
}
