using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using SarasBloggAPI.DTOs.Tarot;

namespace SarasBloggAPI.Services.Tarot;

public class TarotService
{
    private readonly IConfiguration _configuration;
    private readonly HttpClient _httpClient = new HttpClient();

    public TarotService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public async Task<string> InterpretAsync(TarotInterpretRequest request)
    {
        var prompt = BuildPrompt(request);

        // TEMP: return prompt for testing before hooking up OpenAI
        return await CallOpenAI(prompt);
    }

    private string BuildPrompt(TarotInterpretRequest request)
    {
        var cardsText = string.Join(", ", request.Cards);
        var languageInstruction = request.Language == "sv"
            ? "Respond in Swedish."
            : "Respond in English.";

        return $@"
            {languageInstruction}
            User question: {request.Question}

            Cards drawn: {cardsText}

            Provide a reflective tarot interpretation.
            - Do not predict the future
            - Do not give absolute answers
            - Speak directly to the user
            - Use a calm, reflective and slightly poetic tone
            - Keep the response concise (max 150 words)
            - Write in plain text
            - Do not use markdown, bullet points, or symbols
            - Use short paragraphs
            - First give the interpretation, then end with one reflective question
            - When multiple cards are drawn, reflect on how they relate to each other
            - Treat the cards as parts of a single story rather than separate meanings
            - Avoid generic opening phrases like ""It feels like"" or ""This suggests""
            - Avoid repeating similar sentence structures
            - Focus on personal reflection rather than explaining the cards
            ";
    }
    
    private async Task<string> CallOpenAI(string prompt)
    {
        var apiKey = _configuration["OpenAI:ApiKey"];

        if (string.IsNullOrEmpty(apiKey))
            throw new Exception("OPENAI_API_KEY is missing");

        var requestBody = new
        {
            model = "gpt-4o-mini",
            messages = new[]
            {
                new { role = "user", content = prompt }
            }
        };

        var json = JsonSerializer.Serialize(requestBody);

        var request = new HttpRequestMessage(HttpMethod.Post, "https://api.openai.com/v1/chat/completions");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
        request.Content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await _httpClient.SendAsync(request);
        var responseContent = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
            throw new Exception($"OpenAI error: {responseContent}");

        using var doc = JsonDocument.Parse(responseContent);
        var content = doc.RootElement
            .GetProperty("choices")[0]
            .GetProperty("message")
            .GetProperty("content")
            .GetString();

        return content ?? "No response from AI.";
    }
}