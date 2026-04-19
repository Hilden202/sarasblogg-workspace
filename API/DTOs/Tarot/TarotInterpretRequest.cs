using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
namespace SarasBloggAPI.DTOs.Tarot;

public class TarotInterpretRequest
{
    public string Question { get; set; } = string.Empty;
    [MinLength(1, ErrorMessage = "At least 1 card is required.")]
    [MaxLength(3, ErrorMessage = "Maximum 3 cards allowed.")]
    public List<string> Cards { get; set; } = new();
    public string Language { get; set; } = "en";
}