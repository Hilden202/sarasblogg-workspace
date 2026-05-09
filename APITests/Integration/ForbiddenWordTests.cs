using System.Net;
using System.Net.Http.Json;
using APITests.Infrastructure;
using APITests.TestHelpers;
using Microsoft.Extensions.DependencyInjection;
using SarasBloggAPI;
using SarasBloggAPI.Data;
using SarasBloggAPI.Models;

namespace APITests.Integration;

public class ForbiddenWordTests : IClassFixture<CustomWebApplicationFactory<Program>>
{
    private readonly CustomWebApplicationFactory<Program> _factory;

    public ForbiddenWordTests(CustomWebApplicationFactory<Program> factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Post_ForbiddenWord_NormalizesPlainWords()
    {
        await ResetForbiddenWordsAsync();
        var client = CreateModeratorClient();

        var response = await client.PostAsJsonAsync("/api/forbiddenword", new ForbiddenWord
        {
            WordPattern = "saga"
        });

        var created = await response.Content.ReadFromJsonAsync<ForbiddenWord>();

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.NotNull(created);
        Assert.Equal("[s$5][a4@][g9][a4@]", created!.WordPattern);
    }

    [Fact]
    public async Task Post_ForbiddenWord_PreservesAlreadyNormalizedPatterns()
    {
        await ResetForbiddenWordsAsync();
        var client = CreateModeratorClient();

        var normalized = "[s$5][a4@][g9][a4@]";
        var response = await client.PostAsJsonAsync("/api/forbiddenword", new ForbiddenWord
        {
            WordPattern = normalized
        });

        var created = await response.Content.ReadFromJsonAsync<ForbiddenWord>();

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.NotNull(created);
        Assert.Equal(normalized, created!.WordPattern);
    }

    private HttpClient CreateModeratorClient()
    {
        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Add(TestAuthHandler.UserIdHeader, "moderator-user");
        client.DefaultRequestHeaders.Add(TestAuthHandler.UserNameHeader, "Moderator");
        client.DefaultRequestHeaders.Add(TestAuthHandler.RolesHeader, "admin");
        return client;
    }

    private async Task ResetForbiddenWordsAsync()
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<MyDbContext>();

        db.ForbiddenWords.RemoveRange(db.ForbiddenWords);
        await db.SaveChangesAsync();
    }
}
