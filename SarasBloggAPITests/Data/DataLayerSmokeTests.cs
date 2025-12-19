using Microsoft.Extensions.DependencyInjection;
using SarasBloggAPI;
using SarasBloggAPI.Data;
using SarasBloggAPITests.Infrastructure;
using Xunit;

namespace SarasBloggAPITests.Data;

public class DataLayerSmokeTests
    : IClassFixture<CustomWebApplicationFactory<Program>>
{
    private readonly CustomWebApplicationFactory<Program> _factory;

    public DataLayerSmokeTests(CustomWebApplicationFactory<Program> factory)
    {
        _factory = factory;
    }

    [Fact]
    public void DbContext_CanBeResolved_FromDI()
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<MyDbContext>();

        Assert.NotNull(db);
    }
}