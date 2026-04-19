using System.Collections.Generic;
namespace SarasBloggAPI.DTOs.Tarot;

public class TarotInterpretRequest
{
    public string Question { get; set; } = string.Empty;
    public List<string> Cards { get; set; } = new();
    public string Language { get; set; } = "en";
}