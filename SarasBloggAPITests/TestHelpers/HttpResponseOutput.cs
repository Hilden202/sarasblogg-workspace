using System.Net.Http;
using Xunit.Abstractions;

namespace SarasBloggAPITests.TestHelpers;

public static class HttpResponseOutput
{
    public static async Task WriteAsync(
        ITestOutputHelper output,
        HttpResponseMessage response,
        string? bodyPreview = null)
    {
        output.WriteLine("=== HTTP RESPONSE ===");
        output.WriteLine($"StatusCode : {(int)response.StatusCode} ({response.StatusCode})");
        output.WriteLine($"ContentType: {response.Content.Headers.ContentType}");
        output.WriteLine($"Length     : {response.Content.Headers.ContentLength}");

        if (bodyPreview is not null)
        {
            output.WriteLine("--- Body Preview ---");
            output.WriteLine(bodyPreview);
        }
    }
}