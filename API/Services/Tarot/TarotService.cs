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
        var mode = request.Mode ?? "soft";

        if (mode == "direct")
        {
            return $@"
            {languageInstruction}
            User question: {request.Question}

            Cards drawn: {cardsText}

            Provide a clear and direct tarot interpretation.

            Tone and voice:
            - Be clear and decisive
            - Lean toward a strong interpretation rather than multiple possibilities
            - Reduce hesitation and avoid soft qualifiers like ""maybe"" or ""perhaps""
            - Speak directly and with quiet confidence
            - You may point toward a likely direction, but do not frame it as certain or inevitable
            - Do not dilute the message to make it more comfortable
            - Avoid poetic or overly abstract language
            - Keep the message focused and to the point
            - Do not present the future as fixed or guaranteed
            - Avoid absolute or deterministic claims

            Structure:
            - Keep the response concise (max 120 words)
            - Write in plain text
            - Do not use markdown, bullet points, or symbols
            - Use short paragraphs
            - Give a clear interpretation first, then end with one sharp reflective question
            - Focus on what is actually happening in the situation rather than explaining the cards
            - Adapt wording to the number of cards drawn

            Card interpretation:
            - When multiple cards are drawn, identify the dominant direction or tension between them
            - Treat the cards as parts of a single situation rather than separate meanings
            - Make the overall direction feel clear and grounded
            - Do not soften conflicting signals unnecessarily

            Card count guidance:
            - For 2 cards, make the contrast or relationship between them clear
            - For 3 cards, show progression: what shaped the situation, what is happening now, and where things are heading
            - Keep the progression clear without labeling it explicitly
            ";
        }

        return $@"
            {languageInstruction}
            User question: {request.Question}

            Cards drawn: {cardsText}

            Provide a reflective tarot interpretation.

            Tone and voice:
            - Do not predict the future
            - Do not give absolute answers
            - Speak directly to the user
            - Use a calm, reflective and slightly poetic tone
            - Let the interpretation feel present and grounded rather than vague
            - Avoid excessive hedging such as ""maybe"", ""perhaps"", or ""it could be"" unless genuinely needed
            - Avoid generic opening phrases like ""It feels like"", ""This suggests"", or similar filler
            - Avoid repeating similar sentence structures

            Structure:
            - Keep the response concise (max 150 words)
            - Write in plain text
            - Do not use markdown, bullet points, or symbols
            - Use short paragraphs
            - First give the interpretation, then end with one reflective question
            - Focus on personal reflection rather than explaining the cards
            - Avoid generic reflective phrases like ""reflect on how..."" unless they add something specific
            - Adapt wording to the number of cards drawn (do not refer to multiple cards if only one is drawn)

            Card interpretation:
            - When multiple cards are drawn, reflect on how they relate to each other
            - Treat the cards as parts of a single story rather than separate meanings
            - Make the relationship, tension, or movement between the cards noticeable
            - Do not force a card into a negative or blocking role if it naturally represents something supportive or connecting
            - Let the interpretation stay true to the core meaning of each card

            Card count guidance:
            - For 2 cards, make each card's role clear while still keeping the interpretation flowing naturally
            - For 3 cards, make the progression clear: the first card shows what has shaped the situation, the second card shows the current state, and the third card shows the direction or development
            - For 3 cards, keep that progression noticeable without using headings or labels
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