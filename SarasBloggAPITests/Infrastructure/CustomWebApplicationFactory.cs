using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SarasBloggAPI.Data;
using Testcontainers.PostgreSql;

namespace SarasBloggAPITests.Infrastructure;

public class CustomWebApplicationFactory<TProgram>
    : WebApplicationFactory<TProgram>
    where TProgram : class
{
    private static string CreateTestPassword()
    {
        return Guid.NewGuid().ToString("N");
    }

    private readonly PostgreSqlContainer _postgresContainer =
        new PostgreSqlBuilder()
            .WithImage("postgres:16-alpine")
            .WithDatabase("sarasblogg_test")
            .WithUsername("testuser") // ok: inte password-pattern
            .WithPassword(CreateTestPassword())
            .Build();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        // 🔹 VIKTIGT: sätt miljön FÖRST
        builder.UseEnvironment("Test");

        // 🔹 Starta containern här (inte i ctor)
        _postgresContainer.StartAsync().GetAwaiter().GetResult();

        builder.ConfigureAppConfiguration(config =>
        {
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:DefaultConnection"] =
                    _postgresContainer.GetConnectionString()
            });
        });

        builder.ConfigureServices(services =>
        {
            // Ta bort original DbContext
            var descriptor = services.SingleOrDefault(d =>
                d.ServiceType == typeof(DbContextOptions<MyDbContext>));

            if (descriptor != null)
                services.Remove(descriptor);

            // Lägg till test-DbContext
            services.AddDbContext<MyDbContext>(options =>
            {
                options.UseNpgsql(_postgresContainer.GetConnectionString());
            });

            // Kör migrationer
            var sp = services.BuildServiceProvider();
            using var scope = sp.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<MyDbContext>();
            db.Database.Migrate();
        });
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
