using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using APITests.Infrastructure;
using APITests.TestHelpers;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using SarasBloggAPI;
using SarasBloggAPI.Data;
using SarasBloggAPI.DTOs.Comment;
using SarasBloggAPI.Models;

namespace APITests.Integration;

public class CommentCreationTests : IClassFixture<CustomWebApplicationFactory<Program>>
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    private readonly HttpClient _client;
    private readonly CustomWebApplicationFactory<Program> _factory;

    public CommentCreationTests(CustomWebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
        _factory = factory;
    }

    [Fact]
    public async Task Post_Comment_AllowsAnonymousCommentAndSetsServerFields()
    {
        await ResetCommentDataAsync();
        var bloggId = await SeedBloggAsync();

        var response = await _client.PostAsJsonAsync("/api/comment", new CommentCreateRequest
        {
            BloggId = bloggId,
            Name = "",
            Content = "En anonym kommentar"
        });

        var dto = await response.Content.ReadFromJsonAsync<CommentDto>(JsonOptions);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(dto);
        Assert.True(dto!.Id > 0);
        Assert.Equal(bloggId, dto.BloggId);
        Assert.Equal("Gäst", dto.Name);
        Assert.Equal("En anonym kommentar", dto.Content);
        Assert.False(dto.OwnedByCurrentUser);
        Assert.False(dto.CanDelete);
        Assert.True(DateTime.UtcNow - dto.CreatedAt < TimeSpan.FromMinutes(1));

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<MyDbContext>();
        var stored = await db.Comments.FindAsync(dto.Id);
        Assert.NotNull(stored);
        Assert.Null(stored!.UserId);
        Assert.Null(stored.Email);
    }

    [Fact]
    public async Task Post_Comment_UsesAuthenticatedUserIdentity()
    {
        await ResetCommentDataAsync();
        var bloggId = await SeedBloggAsync();
        var user = await CreateUserAsync();
        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Add(TestAuthHandler.UserIdHeader, user.Id);
        client.DefaultRequestHeaders.Add(TestAuthHandler.UserNameHeader, user.UserName);

        var response = await client.PostAsJsonAsync("/api/comment", new CommentCreateRequest
        {
            BloggId = bloggId,
            Name = "Spoofed Name",
            Content = "En inloggad kommentar"
        });

        var dto = await response.Content.ReadFromJsonAsync<CommentDto>(JsonOptions);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(dto);
        Assert.Equal(user.UserName, dto!.Name);
        Assert.True(dto.OwnedByCurrentUser);
        Assert.True(dto.CanDelete);

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<MyDbContext>();
        var stored = await db.Comments.FindAsync(dto.Id);
        Assert.NotNull(stored);
        Assert.Equal(user.Id, stored!.UserId);
        Assert.Null(stored.Email);
    }

    [Fact]
    public async Task Post_Comment_RejectsLegacyWriteFields()
    {
        await ResetCommentDataAsync();
        var bloggId = await SeedBloggAsync();

        var response = await _client.PostAsJsonAsync("/api/comment", new
        {
            id = 123,
            bloggId,
            name = "Besökare",
            email = "legacy@example.com",
            content = "Legacy shaped comment",
            createdAt = DateTime.UtcNow
        });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<MyDbContext>();
        Assert.Empty(db.Comments);
    }

    [Fact]
    public async Task Post_Comment_RejectsForbiddenWords()
    {
        await ResetCommentDataAsync();
        var bloggId = await SeedBloggAsync();

        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<MyDbContext>();
            db.ForbiddenWords.Add(new ForbiddenWord { WordPattern = "blockedword" });
            await db.SaveChangesAsync();
        }

        var response = await _client.PostAsJsonAsync("/api/comment", new CommentCreateRequest
        {
            BloggId = bloggId,
            Name = "Besökare",
            Content = "Det här innehåller blockedword"
        });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        using var verifyScope = _factory.Services.CreateScope();
        var verifyDb = verifyScope.ServiceProvider.GetRequiredService<MyDbContext>();
        Assert.Empty(verifyDb.Comments);
    }

    [Fact]
    public async Task Post_Comment_RejectsHiddenAndFutureBloggs()
    {
        await ResetCommentDataAsync();
        var hiddenId = await SeedBloggAsync(hidden: true);
        var futureId = await SeedBloggAsync(launchDate: DateTime.UtcNow.AddDays(1));

        var hiddenResponse = await _client.PostAsJsonAsync("/api/comment", new CommentCreateRequest
        {
            BloggId = hiddenId,
            Name = "Besökare",
            Content = "Kommentar till dolt inlägg"
        });

        var futureResponse = await _client.PostAsJsonAsync("/api/comment", new CommentCreateRequest
        {
            BloggId = futureId,
            Name = "Besökare",
            Content = "Kommentar till framtida inlägg"
        });

        Assert.Equal(HttpStatusCode.NotFound, hiddenResponse.StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, futureResponse.StatusCode);
    }

    private async Task ResetCommentDataAsync()
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<MyDbContext>();

        db.Comments.RemoveRange(db.Comments);
        db.ForbiddenWords.RemoveRange(db.ForbiddenWords);
        db.BloggLikes.RemoveRange(db.BloggLikes);
        db.BloggImages.RemoveRange(db.BloggImages);
        db.Bloggs.RemoveRange(db.Bloggs);

        await db.SaveChangesAsync();
    }

    private async Task<int> SeedBloggAsync(bool hidden = false, DateTime? launchDate = null)
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<MyDbContext>();

        var blogg = new Blogg
        {
            Title = "Comment test blog",
            Content = "<p>Comment test content</p>",
            Author = "IntegrationTest",
            LaunchDate = launchDate ?? DateTime.UtcNow.AddHours(-1),
            Hidden = hidden,
            IsArchived = false
        };

        db.Bloggs.Add(blogg);
        await db.SaveChangesAsync();
        return blogg.Id;
    }

    private async Task<ApplicationUser> CreateUserAsync()
    {
        using var scope = _factory.Services.CreateScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var suffix = Guid.NewGuid().ToString("N");
        var user = new ApplicationUser
        {
            UserName = $"commenter-{suffix}",
            Email = $"commenter-{suffix}@example.com",
            EmailConfirmed = true
        };

        var result = await userManager.CreateAsync(user);
        Assert.True(result.Succeeded, string.Join("; ", result.Errors.Select(e => e.Description)));

        return user;
    }
}
