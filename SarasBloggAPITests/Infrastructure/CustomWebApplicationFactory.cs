using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SarasBloggAPI.Data;
using Testcontainers.PostgreSql;

namespace SarasBloggAPITests.Infrastructure;

public class CustomWebApplicationFactory<TProgram>
    : WebApplicationFactory<TProgram>
    where TProgram : class
{
    private readonly PostgreSqlContainer _postgresContainer =
        new PostgreSqlBuilder()
            .WithImage("postgres:16-alpine")
            .WithDatabase("sarasblogg_test")
            // Test-only credentials (used by Testcontainers, not production)
            .WithUsername(
                Environment.GetEnvironmentVariable("TEST_DB_USER") ?? "testuser"
            )
            .WithPassword(
                Environment.GetEnvironmentVariable("TEST_DB_PASSWORD") ?? "testpassword"
            )

            .Build();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            // Remove real DbContext
            var descriptor = services.SingleOrDefault(d =>
                d.ServiceType == typeof(DbContextOptions<MyDbContext>));

            if (descriptor != null)
            {
                services.Remove(descriptor);
            }

            // Register test DbContext (Postgres)
            services.AddDbContext<MyDbContext>(options =>
            {
                options.UseNpgsql(_postgresContainer.GetConnectionString());
            });
        });
    }

    protected override IHost CreateHost(IHostBuilder builder)
    {
        // Start Postgres container BEFORE host creation
        _postgresContainer.StartAsync().GetAwaiter().GetResult();

        var host = base.CreateHost(builder);

        // Apply migrations
        using var scope = host.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<MyDbContext>();
        db.Database.Migrate();

        return host;
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);

        if (disposing)
        {
            _postgresContainer.DisposeAsync()
                .AsTask()
                .GetAwaiter()
                .GetResult();
        }
    }
}
