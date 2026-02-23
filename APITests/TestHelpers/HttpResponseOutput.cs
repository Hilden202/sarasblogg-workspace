using System.Net.Http;
using System.Text.Json;
using Xunit.Abstractions;

namespace APITests.TestHelpers;

public static class HttpResponseOutput
{
    public static async Task WriteAsync(
        ITestOutputHelper output,
        HttpResponseMessage response,
        string endpoint,
        string method,
        int bodyPreviewLength = 300)
    {
        var status = (int)response.StatusCode;
        var ok = response.IsSuccessStatusCode ? "OK" : "FAIL";

        output.WriteLine("========================================");
        output.WriteLine($"HTTP {method} {endpoint}");
        output.WriteLine("----------------------------------------");
        output.WriteLine($"Result     : {ok}");
        output.WriteLine($"Status     : {status} ({response.StatusCode})");
        output.WriteLine($"ContentType: {response.Content.Headers.ContentType?.MediaType ?? "<none>"}");
        output.WriteLine($"Length     : {response.Content.Headers.ContentLength?.ToString() ?? "<unknown>"}");

        var body = await response.Content.ReadAsStringAsync();

        if (!string.IsNullOrWhiteSpace(body))
        {
            output.WriteLine("----------------------------------------");
            output.WriteLine("Body (preview)");

            var preview = body.Length > bodyPreviewLength
                ? body[..bodyPreviewLength] + " …"
                : body;

            // Försök pretty-print JSON om möjligt
            if (response.Content.Headers.ContentType?.MediaType == "application/json")
            {
                try
                {
                    using var doc = JsonDocument.Parse(body);
                    preview = JsonSerializer.Serialize(doc, new JsonSerializerOptions
                    {
                        WriteIndented = true
                    });

                    if (preview.Length > bodyPreviewLength)
                        preview = preview[..bodyPreviewLength] + " …";
                }
                catch
                {
                    // fall back till rå text
                }
            }

            output.WriteLine(preview);
        }
        else
        {
            output.WriteLine("----------------------------------------");
            output.WriteLine("Body       : <empty>");
        }

        output.WriteLine("========================================");
    }
}