using System.ComponentModel.DataAnnotations;

namespace DSAGrind.Models.DTOs;

// Request DTOs for Admin Controller
public class BanUserRequestDto
{
    [Required]
    public string Reason { get; set; } = string.Empty;
}

public class RejectProblemRequestDto
{
    [Required]
    public string Reason { get; set; } = string.Empty;
}