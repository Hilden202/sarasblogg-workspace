using System.Net;
using SarasBloggAPI;
using SarasBloggAPITests.Infrastructure;
using SarasBloggAPITests.TestHelpers;
using Xunit;
using Xunit.Abstractions;

namespace SarasBloggAPITests.Integration;

public class AuthTests
    : IClassFixture<CustomWebApplicationFactory<Program>>
{
    private readonly HttpClient _client;
    private readonly ITestOutputHelper _output;

    public AuthTests(
        CustomWebApplicationFactory<Program> factory,
        ITestOutputHelper output)
    {
        _client = factory.CreateClient();
        _output = output;
    }

    [Fact]
    public async Task Get_Me_Returns401Or404_WhenAnonymous()
    {
        // Arrange
        var endpoint = "/api/users/me";

        // Act
        var response = await _client.GetAsync(endpoint);
        var actualStatusCode = response.StatusCode;

        // Assert
        Assert.True(
            actualStatusCode == HttpStatusCode.Unauthorized ||
            actualStatusCode == HttpStatusCode.NotFound,
            $"Expected 401 or 404 but got {(int)actualStatusCode} ({actualStatusCode})"
        );

        // Output (för debug / logg)
        var body = await response.Content.ReadAsStringAsync();
        await HttpResponseOutput.WriteAsync(
            _output,
            response,
            endpoint,
            method: "GET"
        );
    }
}