using System.ComponentModel.DataAnnotations;

namespace Wedding_Proposal_BE.Features.Auth.Application.DTOs;

public class ChangePasswordRequest
{
    [Required]
    public string CurrentPassword { get; set; } = string.Empty;

    [Required]
    public string NewPassword { get; set; } = string.Empty;
}
