using System.ComponentModel.DataAnnotations;

namespace Wedding_Proposal_BE.Features.Auth.Application.DTOs;

public class RefreshTokenRequest
{
    [Required]
    public string RefreshToken { get; set; } = string.Empty;
}
