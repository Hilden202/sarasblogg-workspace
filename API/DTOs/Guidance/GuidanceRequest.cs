using System.ComponentModel.DataAnnotations;

namespace SarasBloggAPI.DTOs.Guidance;

public class GuidanceRequest
{
    [Required]
    [StringLength(180, MinimumLength = 1, ErrorMessage = "Skriv ett ord eller en kort fråga.")]
    public string Input { get; set; } = string.Empty;
}
