using System.Net;
using System.Net.Http.Json;
using APITests.Infrastructure;
using APITests.TestHelpers;
using Microsoft.Extensions.DependencyInjection;
using SarasBloggAPI;
using SarasBloggAPI.Data;
using SarasBloggAPI.DTOs.Blogg;
using SarasBloggAPI.Models;

namespace APITests.Integration;

public class BlogPostWriteTests : IClassFixture<CustomWebApplicationFactory<Program>>
{
    private readonly CustomWebApplicationFactory<Program> _factory;

    public BlogPostWriteTests(CustomWebApplicationFactory<Program> factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Post_Blogg_CreatesPostFromWriteDto()
    {
        await ResetBlogDataAsync();
        var client = CreateAdminClient(userId: "creator-user", userName: "Creator");

        var response = await client.PostAsJsonAsync("/api/blogg", new BlogPostWriteRequest
        {
            Title = "API owned post",
            Content = "<p>Hello <strong>world</strong></p>",
            Author = "Sara",
            LaunchDateLocal = new DateTime(2026, 1, 15, 9, 30, 0),
            Hidden = false,
            IsArchived = false
        });

        var created = await response.Content.ReadFromJsonAsync<Blogg>();

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.NotNull(created);
        Assert.Equal("API owned post", created!.Title);
        Assert.Equal("<p>Hello <strong>world</strong></p>", created.Content);
        Assert.Equal("Sara", created.Author);
        Assert.Equal("creator-user", created.UserId);
        Assert.Equal(0, created.ViewCount);
        Assert.False(created.Hidden);
        Assert.False(created.IsArchived);
    }

    [Fact]
    public async Task Put_Blogg_UpdatesAllowedFieldsAndPreservesServerOwnedFields()
    {
        await ResetBlogDataAsync();
        var bloggId = await SeedBloggAsync(userId: "original-owner", viewCount: 42);
        var client = CreateAdminClient(userId: "different-admin", userName: "Admin");

        var response = await client.PutAsJsonAsync($"/api/blogg/{bloggId}", new BlogPostWriteRequest
        {
            Title = "Updated title",
            Content = "<p>Updated content</p>",
            Author = "Updated author",
            LaunchDateLocal = new DateTime(2026, 2, 10, 12, 0, 0),
            Hidden = true,
            IsArchived = true
        });

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

        var stored = await FindBloggAsync(bloggId);
        Assert.NotNull(stored);
        Assert.Equal("Updated title", stored!.Title);
        Assert.Equal("<p>Updated content</p>", stored.Content);
        Assert.Equal("Updated author", stored.Author);
        Assert.True(stored.Hidden);
        Assert.True(stored.IsArchived);
        Assert.Equal("original-owner", stored.UserId);
        Assert.Equal(42, stored.ViewCount);
    }

    [Fact]
    public async Task Post_Blogg_GeneratesFallbackTitleWhenMissing()
    {
        await ResetBlogDataAsync();
        var client = CreateAdminClient();

        var response = await client.PostAsJsonAsync("/api/blogg", new BlogPostWriteRequest
        {
            Title = " ",
            Content = "<p>Det här blir titel från innehållet</p>",
            Author = "Sara",
            LaunchDateLocal = new DateTime(2026, 1, 15, 9, 30, 0)
        });

        var created = await response.Content.ReadFromJsonAsync<Blogg>();

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.NotNull(created);
        Assert.Equal("Det här blir titel från innehållet", created!.Title);
    }

    [Fact]
    public async Task Post_Blogg_ConvertsLaunchDateLocalToUtc()
    {
        await ResetBlogDataAsync();
        var client = CreateAdminClient();

        var response = await client.PostAsJsonAsync("/api/blogg", new BlogPostWriteRequest
        {
            Title = "Date conversion",
            Content = "<p>Date conversion content</p>",
            Author = "Sara",
            LaunchDateLocal = new DateTime(2026, 1, 15, 9, 30, 0)
        });

        var created = await response.Content.ReadFromJsonAsync<Blogg>();

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.NotNull(created);
        Assert.Equal(new DateTime(2026, 1, 15, 8, 30, 0), created!.LaunchDate);
    }

    [Fact]
    public async Task Post_Blogg_RejectsLegacyWriteFields()
    {
        await ResetBlogDataAsync();
        var client = CreateAdminClient(userId: "actual-owner", userName: "ActualOwner");

        var response = await client.PostAsJsonAsync("/api/blogg", new
        {
            title = "Legacy shaped post",
            content = "<p>Legacy shaped content</p>",
            author = "Sara",
            launchDate = new DateTime(2026, 1, 15, 8, 30, 0, DateTimeKind.Utc),
            hidden = false,
            isArchived = false,
            userId = "spoofed-owner",
            viewCount = 999
        });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<MyDbContext>();
        Assert.Empty(db.Bloggs);
    }

    private HttpClient CreateAdminClient(string userId = "admin-user", string userName = "Admin")
    {
        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Add(TestAuthHandler.UserIdHeader, userId);
        client.DefaultRequestHeaders.Add(TestAuthHandler.UserNameHeader, userName);
        client.DefaultRequestHeaders.Add(TestAuthHandler.RolesHeader, "admin");
        return client;
    }

    private async Task ResetBlogDataAsync()
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<MyDbContext>();

        db.BloggLikes.RemoveRange(db.BloggLikes);
        db.Comments.RemoveRange(db.Comments);
        db.BloggImages.RemoveRange(db.BloggImages);
        db.Bloggs.RemoveRange(db.Bloggs);

        await db.SaveChangesAsync();
    }

    private async Task<int> SeedBloggAsync(string userId, int viewCount)
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<MyDbContext>();

        var blogg = new Blogg
        {
            Title = "Original title",
            Content = "<p>Original content</p>",
            Author = "Original author",
            LaunchDate = DateTime.UtcNow.AddDays(-1),
            Hidden = false,
            IsArchived = false,
            UserId = userId,
            ViewCount = viewCount
        };

        db.Bloggs.Add(blogg);
        await db.SaveChangesAsync();
        return blogg.Id;
    }

    private async Task<Blogg?> FindBloggAsync(int id)
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<MyDbContext>();
        return await db.Bloggs.FindAsync(id);
    }
}
