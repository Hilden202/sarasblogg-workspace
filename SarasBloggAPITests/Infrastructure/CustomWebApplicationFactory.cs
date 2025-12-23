using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SarasBloggAPI.Data;
using Testcontainers.PostgreSql;

namespace SarasBloggAPITests.Infrastructure;

public class CustomWebApplicationFactory<TProgram>
    : WebApplicationFactory<TProgram>
    where TProgram : class
{
    private const string TestDbUser = "postgres_test";
    private const string TestDbPassword = "postgres-test-only";

    private readonly PostgreSqlContainer _postgresContainer;

    public CustomWebApplicationFactory()
    {
        _postgresContainer = new PostgreSqlBuilder()
            .WithImage("postgres:16-alpine")
            .WithDatabase("sarasblogg_test")
            .WithUsername(
                Environment.GetEnvironmentVariable("TEST_DB_USER") ?? TestDbUser
            )
            .WithPassword(
                Environment.GetEnvironmentVariable("TEST_DB_PASSWORD") ?? TestDbPassword
            )
            .Build();

        // 🔴 STARTA CONTAINERN HÄR
        _postgresContainer.StartAsync().GetAwaiter().GetResult();
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            // Ta bort befintlig DbContext
            var descriptor = services.SingleOrDefault(d =>
                d.ServiceType == typeof(DbContextOptions<MyDbContext>));

            if (descriptor != null)
            {
                services.Remove(descriptor);
            }

            // Lägg till test-DbContext
            services.AddDbContext<MyDbContext>(options =>
            {
                options.UseNpgsql(_postgresContainer.GetConnectionString());
            });

            // 🔴 Kör migrationer EFTER att DI är klart
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