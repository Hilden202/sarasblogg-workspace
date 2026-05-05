using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using APITests.Infrastructure;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using SarasBloggAPI;
using SarasBloggAPI.Data;
using SarasBloggAPI.DTOs.Blogg;
using SarasBloggAPI.Models;

namespace APITests.Integration;

public class PublicBloggTests : IClassFixture<CustomWebApplicationFactory<Program>>
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    private readonly HttpClient _client;
    private readonly CustomWebApplicationFactory<Program> _factory;

    public PublicBloggTests(CustomWebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
        _factory = factory;
    }

    [Fact]
    public async Task Get_PublicBloggs_ReturnsOnlyVisiblePublishedActivePosts()
    {
        await ResetBlogDataAsync();
        await SeedVisibilityPostsAsync();

        var response = await _client.GetAsync("/api/blogg/public?page=1&pageSize=10");
        var result = await ReadListResponseAsync(response);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(2, result.TotalItems);
        Assert.Equal(1, result.TotalPages);
        Assert.Equal(new[] { "Latest public", "Older public" }, result.Items.Select(i => i.Title));
        Assert.DoesNotContain(result.Items, i => i.Title is "Hidden public" or "Future public" or "Archived public");
    }

    [Fact]
    public async Task Get_PublicBloggs_AppliesPaginationAndCoverImage()
    {
        await ResetBlogDataAsync();
        await SeedVisibilityPostsAsync();

        var response = await _client.GetAsync("/api/blogg/public?page=1&pageSize=1");
        var result = await ReadListResponseAsync(response);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(1, result.Page);
        Assert.Equal(1, result.PageSize);
        Assert.Equal(2, result.TotalItems);
        Assert.Equal(2, result.TotalPages);
        Assert.Single(result.Items);
        Assert.Equal("Latest public", result.Items[0].Title);
        Assert.Equal("/uploads/blogg/latest-cover-first.jpg", result.Items[0].CoverImage?.FilePath);
    }

    [Fact]
    public async Task Get_PublicBloggs_ReturnsArchivedPostsWhenArchiveFilterIsTrue()
    {
        await ResetBlogDataAsync();
        await SeedVisibilityPostsAsync();

        var response = await _client.GetAsync("/api/blogg/public?archive=true&page=1&pageSize=10");
        var result = await ReadListResponseAsync(response);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Single(result.Items);
        Assert.Equal("Archived public", result.Items[0].Title);
        Assert.True(result.Items[0].IsArchived);
    }

    [Fact]
    public async Task Get_PublicBloggByIdOrSlug_ReturnsDetailWithOrderedImages()
    {
        await ResetBlogDataAsync();
        await SeedVisibilityPostsAsync();
        var list = await _client.GetFromJsonAsync<BlogPostListDto>("/api/blogg/public?page=1&pageSize=10", JsonOptions);
        var slug = Assert.Single(list!.Items, i => i.Title == "Latest public").Slug;

        var response = await _client.GetAsync($"/api/blogg/public/{slug}");
        var detail = await response.Content.ReadFromJsonAsync<BlogPostDetailDto>(JsonOptions);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(detail);
        Assert.Equal("Latest public", detail!.Title);
        Assert.Equal("<p>Latest public content</p>", detail.Content);
        Assert.Equal("/uploads/blogg/latest-cover-first.jpg", detail.CoverImage?.FilePath);
        Assert.Equal(
            new[] { "/uploads/blogg/latest-cover-first.jpg", "/uploads/blogg/latest-cover-second.jpg" },
            detail.Images.Select(i => i.FilePath));
    }

    [Fact]
    public async Task Get_PublicBloggByIdOrSlug_Returns404ForHiddenOrFuturePosts()
    {
        await ResetBlogDataAsync();
        var ids = await SeedVisibilityPostsAsync();

        var hiddenResponse = await _client.GetAsync($"/api/blogg/public/{ids.HiddenId}");
        var futureResponse = await _client.GetAsync($"/api/blogg/public/{ids.FutureId}");

        Assert.Equal(HttpStatusCode.NotFound, hiddenResponse.StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, futureResponse.StatusCode);
    }

    private async Task<BlogPostListDto> ReadListResponseAsync(HttpResponseMessage response)
    {
        var result = await response.Content.ReadFromJsonAsync<BlogPostListDto>(JsonOptions);
        Assert.NotNull(result);
        return result!;
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

    private async Task<(int HiddenId, int FutureId)> SeedVisibilityPostsAsync()
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<MyDbContext>();
        var now = DateTime.UtcNow;

        var olderPublic = new Blogg
        {
            Title = "Older public",
            Content = "<p>Older public content</p>",
            Author = "IntegrationTest",
            LaunchDate = now.AddHours(-3),
            IsArchived = false,
            Hidden = false
        };

        var latestPublic = new Blogg
        {
            Title = "Latest public",
            Content = "<p>Latest public content</p>",
            Author = "IntegrationTest",
            LaunchDate = now.AddHours(-1),
            IsArchived = false,
            Hidden = false,
            Images = new List<BloggImage>
            {
                new() { FilePath = "/uploads/blogg/latest-cover-second.jpg", Order = 2 },
                new() { FilePath = "/uploads/blogg/latest-cover-first.jpg", Order = 1 }
            }
        };

        var hidden = new Blogg
        {
            Title = "Hidden public",
            Content = "<p>Hidden content</p>",
            Author = "IntegrationTest",
            LaunchDate = now.AddHours(-2),
            IsArchived = false,
            Hidden = true
        };

        var future = new Blogg
        {
            Title = "Future public",
            Content = "<p>Future content</p>",
            Author = "IntegrationTest",
            LaunchDate = now.AddDays(1),
            IsArchived = false,
            Hidden = false
        };

        var archived = new Blogg
        {
            Title = "Archived public",
            Content = "<p>Archived content</p>",
            Author = "IntegrationTest",
            LaunchDate = now.AddHours(-4),
            IsArchived = true,
            Hidden = false
        };

        db.Bloggs.AddRange(olderPublic, latestPublic, hidden, future, archived);
        await db.SaveChangesAsync();

        return (hidden.Id, future.Id);
    }
}
