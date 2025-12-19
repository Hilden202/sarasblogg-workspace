using System.Net;
using SarasBloggAPI;
using SarasBloggAPITests.Infrastructure;
using SarasBloggAPITests.TestHelpers;
using Xunit;
using Xunit.Abstractions;

namespace SarasBloggAPITests.Integration;

public class BloggTests
    : IClassFixture<CustomWebApplicationFactory<Program>>
{
    private readonly HttpClient _client;
    private readonly ITestOutputHelper _output;

    public BloggTests(
        CustomWebApplicationFactory<Program> factory,
        ITestOutputHelper output)
    {
        _client = factory.CreateClient();
        _output = output;
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
            body[..Math.Min(body.Length, 300)]
        );
    }
}