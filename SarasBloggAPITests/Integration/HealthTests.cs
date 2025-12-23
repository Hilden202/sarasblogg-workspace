using System.Net;
using Microsoft.AspNetCore.Mvc.Testing;
using SarasBloggAPI;
using SarasBloggAPITests.Infrastructure;
using SarasBloggAPITests.TestHelpers;
using Xunit;
using Xunit.Abstractions;

namespace SarasBloggAPITests.Integration;

public class HealthTests
    : IClassFixture<CustomWebApplicationFactory<Program>>
{
    private readonly HttpClient _client;
    private readonly ITestOutputHelper _output;

    public HealthTests(
        CustomWebApplicationFactory<Program> factory,
        ITestOutputHelper output)
    {
        _client = factory.CreateClient();
        _output = output;
    }

    [Fact]
    public async Task Api_Starts_WithoutErrors()
    {
        // Arrange
        var endpoint = "/health/db";

        // Act
        var response = await _client.GetAsync(endpoint);

        // Assert
        response.EnsureSuccessStatusCode();

        // Output
        var body = await response.Content.ReadAsStringAsync();
        await HttpResponseOutput.WriteAsync(
            _output,
            response,
            endpoint,
            method: "GET"
        );
    }
    [Fact]
    public async Task Api_IsAlive()
    {
        var response = await _client.GetAsync("/health");
        response.EnsureSuccessStatusCode();
    }
}