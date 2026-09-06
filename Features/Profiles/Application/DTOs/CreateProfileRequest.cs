using System.ComponentModel.DataAnnotations;

namespace Wedding_Proposal_BE.Features.Profiles.Application.DTOs;

public class CreateProfileRequest
{
    [Required, MaxLength(50)]
    public string FirstName { get; set; } = string.Empty;

    [Required, MaxLength(50)]
    public string LastName { get; set; } = string.Empty;

    [Required]
    public DateOnly DateOfBirth { get; set; }
}
