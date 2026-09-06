using System.ComponentModel.DataAnnotations;

namespace Wedding_Proposal_BE.Features.Auth.Application.DTOs;

public class ResendRegistrationOtpRequest
{
    [Required, EmailAddress]
    public string Email { get; set; } = string.Empty;
}
