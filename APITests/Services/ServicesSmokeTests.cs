using Microsoft.Extensions.DependencyInjection;
using SarasBloggAPI;
using SarasBloggAPI.Services;
using APITests.Infrastructure;
using Xunit;

namespace APITests.Services;

public class ServicesSmokeTests
    : IClassFixture<CustomWebApplicationFactory<Program>>
{
    private readonly CustomWebApplicationFactory<Program> _factory;

    public ServicesSmokeTests(CustomWebApplicationFactory<Program> factory)
    {
        _factory = factory;
    }

    [Fact]
    public void Services_AreResolvable_FromDI()
    {
        using var scope = _factory.Services.CreateScope();

        var service = scope.ServiceProvider.GetService<TokenService>();

        Assert.NotNull(service);
    }
}