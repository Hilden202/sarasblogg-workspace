using Microsoft.AspNetCore.Mvc.Testing;
using System.Net;
using System.Net.Http.Json;
using Microsoft.Extensions.DependencyInjection;
using SarasBloggAPI;
using SarasBloggAPI.Data;
using SarasBloggAPI.Models;
using APITests.Infrastructure;
using APITests.TestHelpers;
using Xunit;
using Xunit.Abstractions;

namespace APITests.Integration;

public class BloggTests
    : IClassFixture<CustomWebApplicationFactory<Program>>
{
    private readonly HttpClient _client;
    private readonly ITestOutputHelper _output;
    private readonly CustomWebApplicationFactory<Program> _factory;


    public BloggTests(
        CustomWebApplicationFactory<Program> factory,
        ITestOutputHelper output)
    {
        _client = factory.CreateClient();
        _output = output;
        _factory = factory;

    }

    [Fact]
    public async Task Get_Bloggs_ReturnsOk_AndJson()
    {
        // Arrange
        var endpoint = "/api/blogg";

        // Act
        var response = await _client.GetAsync(endpoint);

        // Assert
        response.EnsureSuccessStatusCode();
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("application/json", response.Content.Headers.ContentType?.MediaType);

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
    public async Task Get_BloggById_Returns404_WhenNotFound()
    {
        // Arrange
        var bloggId = 999999;
        var endpoint = $"/api/blogg/{bloggId}";
        var expectedStatusCode = HttpStatusCode.NotFound;

        // Act
        var response = await _client.GetAsync(endpoint);
        var actualStatusCode = response.StatusCode;

        // Assert
        Assert.Equal(expectedStatusCode, actualStatusCode);

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
    public async Task Get_BloggById_Returns200_WhenExists()
    {
        // Arrange
        var expectedStatusCode = HttpStatusCode.OK;
        int bloggId;
        
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<MyDbContext>();

            var blogg = new Blogg
            {
                Title = "Test blog",
                Content = "Integration test content",
                Author = "IntegrationTest",
                LaunchDate = DateTime.UtcNow,
                IsArchived = false,
                Hidden = false
            };

            db.Bloggs.Add(blogg);
            await db.SaveChangesAsync();

            bloggId = blogg.Id;
        }

        var endpoint = $"/api/blogg/{bloggId}";

        // Act
        var response = await _client.GetAsync(endpoint);
        var actualStatusCode = response.StatusCode;

        // Assert
        Assert.Equal(expectedStatusCode, actualStatusCode);
        Assert.Equal("application/json", response.Content.Headers.ContentType?.MediaType);

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
    public async Task Post_Blogg_Returns401_WhenAnonymous()
    {
        // Arrange
        var endpoint = "/api/blogg";
        var expectedStatusCode = HttpStatusCode.Unauthorized;

        var payload = new
        {
            title = "Blocked blog",
            content = "Should not be created",
            author = "IntegrationTest",
            launchDate = DateTime.UtcNow
        };

        // Act
        var response = await _client.PostAsJsonAsync(endpoint, payload);
        var actualStatusCode = response.StatusCode;

        // Assert
        Assert.Equal(expectedStatusCode, actualStatusCode);

        // Output
        await HttpResponseOutput.WriteAsync(
            _output,
            response,
            endpoint,
            method: "POST"
        );
    }
    [Fact]
    public async Task Get_Bloggs_ReturnsEmptyList_WhenDatabaseIsEmpty()
    {
        // Arrange
        var endpoint = "/api/blogg";
        var expectedStatusCode = HttpStatusCode.OK;
        var expectedBody = "[]";

        // Act
        var response = await _client.GetAsync(endpoint);
        var actualStatusCode = response.StatusCode;
        var body = await response.Content.ReadAsStringAsync();

        // Assert
        Assert.Equal(expectedStatusCode, actualStatusCode);
        Assert.Equal("application/json", response.Content.Headers.ContentType?.MediaType);
        Assert.Equal(expectedBody, body.Trim());

        // Output
        await HttpResponseOutput.WriteAsync(
            _output,
            response,
            endpoint,
            method: "GET"
        );
    }
}