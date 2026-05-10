using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using SarasBloggAPI.DTOs.Guidance;

namespace SarasBloggAPI.Services.Guidance;

public class GuidanceService
{
    private const string OpenAiEndpoint = "https://api.openai.com/v1/chat/completions";
    private readonly IConfiguration _configuration;
    private readonly HttpClient _httpClient = new() { Timeout = TimeSpan.FromSeconds(12) };

    public GuidanceService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public async Task<GuidanceResponse> GenerateAsync(GuidanceRequest request, CancellationToken cancellationToken)
    {
        var input = NormalizeInput(request.Input);
        var response = await CallOpenAI(input, cancellationToken);

        return new GuidanceResponse
        {
            Guidance = CleanResponse(response)
        };
    }

    private static string NormalizeInput(string input)
    {
        return Regex.Replace(input.Trim(), @"\s+", " ");
    }

    private static object[] BuildMessages(string input)
    {
        return
        [
            new
            {
                role = "system",
                content = """
Du skriver små vägledande reflektioner för en svensk blogg.
Svara alltid endast på svenska.
Svara med 1-3 korta meningar och högst ungefär 45 ord.
Tonen ska vara varm, lugn, jordnära, personlig och lätt poetisk.
Ta användarens ord eller fråga på allvar och låt svaret kännas specifikt för det.
Skriv som en liten inre kompass, inte som en AI-assistent, chatbot, terapeut eller spådam.
Ge inga säkra framtidslöften, ingen diagnos och ingen medicinsk, juridisk eller ekonomisk rådgivning.
Använd inte markdown, rubriker, listor, emojis eller citattecken runt svaret.
Undvik fraser som "som AI", "jag kan hjälpa dig", "det beror på" och generiskt fyllnadsspråk.
"""
            },
            new
            {
                role = "user",
                content = $"""
Användarens ord eller fråga:
{input}

Skriv en kort reflektion som svarar på detta.
"""
            }
        ];
    }

    private async Task<string> CallOpenAI(string input, CancellationToken cancellationToken)
    {
        var apiKey = _configuration["OpenAI:ApiKey"];

        if (string.IsNullOrWhiteSpace(apiKey))
            throw new InvalidOperationException("OpenAI API key is missing.");

        var requestBody = new
        {
            model = "gpt-4o-mini",
            messages = BuildMessages(input),
            temperature = 0.75,
            max_tokens = 90
        };

        var json = JsonSerializer.Serialize(requestBody);

        using var request = new HttpRequestMessage(HttpMethod.Post, OpenAiEndpoint);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
        request.Content = new StringContent(json, Encoding.UTF8, "application/json");

        using var response = await _httpClient.SendAsync(request, cancellationToken);
        var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);

        if (!response.IsSuccessStatusCode)
            throw new HttpRequestException($"OpenAI guidance request failed: {response.StatusCode}");

        using var doc = JsonDocument.Parse(responseContent);
        var content = doc.RootElement
            .GetProperty("choices")[0]
            .GetProperty("message")
            .GetProperty("content")
            .GetString();

        if (string.IsNullOrWhiteSpace(content))
            throw new InvalidOperationException("OpenAI returned an empty guidance response.");

        return content;
    }

    private static string CleanResponse(string response)
    {
        var lines = response
            .Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(line => Regex.Replace(line, @"^[-*•\d.)\s]+", ""));

        var cleaned = string.Join(" ", lines);
        cleaned = Regex.Replace(cleaned, @"[*_`#>]", "");
        cleaned = cleaned.Trim().Trim('"', '“', '”');
        cleaned = Regex.Replace(cleaned, @"\s+", " ");

        return LimitResponse(cleaned);
    }

    private static string LimitResponse(string response)
    {
        var sentences = Regex.Matches(response, @"[^.!?]+[.!?]+|[^.!?]+$")
            .Select(match => match.Value.Trim())
            .Where(sentence => sentence.Length > 0)
            .Take(3);

        var limited = string.Join(" ", sentences);
        const int maxLength = 360;

        if (limited.Length <= maxLength)
            return limited;

        var cutoff = limited.LastIndexOf(' ', maxLength);
        if (cutoff < 120)
            cutoff = maxLength;

        return limited[..cutoff].TrimEnd(',', ';', ':', ' ') + ".";
    }
}
