using System.ComponentModel.DataAnnotations;

namespace Wedding_Proposal_BE.Features.Auth.Application.DTOs;

public class GoogleAuthCallbackRequest
{
    [Required]
    public string Code { get; set; } = string.Empty;

    [Required]
    public string State { get; set; } = string.Empty;
}
