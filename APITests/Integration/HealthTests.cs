using System.Net;
using Microsoft.AspNetCore.Mvc.Testing;
using SarasBloggAPI;
using APITests.Infrastructure;
using APITests.TestHelpers;
using Xunit;
using Xunit.Abstractions;

namespace APITests.Integration;

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
}