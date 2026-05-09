using System.Security.Claims;
using Ganss.Xss;
using SarasBloggAPI.DAL;
using SarasBloggAPI.DTOs.Blogg;
using BloggModel = SarasBloggAPI.Models.Blogg;

namespace SarasBloggAPI.Services.Blogg
{
    public class BlogPostService
    {
        private static readonly TimeZoneInfo SwedishZone =
            TimeZoneInfo.FindSystemTimeZoneById("Europe/Stockholm");

        private readonly BloggManager _bloggManager;
        private readonly NewPostNotifier _notifier;
        private readonly HtmlSanitizer _sanitizer;

        public BlogPostService(BloggManager bloggManager, NewPostNotifier notifier, HtmlSanitizer sanitizer)
        {
            _bloggManager = bloggManager;
            _notifier = notifier;
            _sanitizer = sanitizer;
        }

        public async Task<BloggModel> CreateAsync(BlogPostWriteRequest request, ClaimsPrincipal user)
        {
            var sanitizedContent = SanitizeContent(request.Content);
            var title = ResolveTitle(request.Title, sanitizedContent, request.ShowTitle);
            var blogg = new BloggModel
            {
                Title = title.Value,
                ShowTitle = title.ShowTitle,
                IsTitleGenerated = title.IsGenerated,
                Content = sanitizedContent,
                Author = ResolveAuthor(request.Author, user),
                LaunchDate = ResolveLaunchDateUtc(request),
                Hidden = request.Hidden,
                IsArchived = request.IsArchived,
                UserId = user.FindFirstValue(ClaimTypes.NameIdentifier),
                ViewCount = 0
            };

            var created = await _bloggManager.CreateAsync(blogg);
            if (!created.Hidden && !created.IsArchived)
            {
                await _notifier.NotifyAsync(created.Id);
            }

            return created;
        }

        public async Task<BloggModel?> UpdateAsync(int id, BlogPostWriteRequest request, ClaimsPrincipal user)
        {
            var existing = await _bloggManager.GetByIdAsync(id);
            if (existing == null)
                return null;

            var sanitizedContent = SanitizeContent(request.Content);
            var title = ResolveTitle(request.Title, sanitizedContent, request.ShowTitle, existing.ShowTitle);

            existing.Title = title.Value;
            existing.ShowTitle = title.ShowTitle;
            existing.IsTitleGenerated = title.IsGenerated;
            existing.Content = sanitizedContent;
            existing.Author = ResolveAuthor(request.Author, user);
            existing.LaunchDate = ResolveLaunchDateUtc(request);
            existing.Hidden = request.Hidden;
            existing.IsArchived = request.IsArchived;

            var updated = await _bloggManager.UpdateAsync(existing);
            return updated ? existing : null;
        }

        private string SanitizeContent(string? content)
        {
            return _sanitizer.Sanitize(content ?? string.Empty);
        }

        private static string ResolveAuthor(string? author, ClaimsPrincipal user)
        {
            if (!string.IsNullOrWhiteSpace(author))
                return author.Trim();

            return user.Identity?.Name ?? string.Empty;
        }

        private static DateTime ResolveLaunchDateUtc(BlogPostWriteRequest request)
        {
            if (request.LaunchDateLocal.HasValue)
            {
                var local = DateTime.SpecifyKind(request.LaunchDateLocal.Value, DateTimeKind.Unspecified);
                return TimeZoneInfo.ConvertTimeToUtc(local, SwedishZone);
            }

            return DateTime.UtcNow;
        }

        private static ResolvedTitle ResolveTitle(
            string? title,
            string content,
            bool? requestedShowTitle,
            bool existingShowTitle = false)
        {
            if (!string.IsNullOrWhiteSpace(title))
            {
                var showTitle = requestedShowTitle ?? existingShowTitle;
                return new ResolvedTitle(title.Trim(), IsGenerated: false, ShowTitle: showTitle);
            }

            return new ResolvedTitle(
                BlogTextHelper.GenerateFallbackTitle(content),
                IsGenerated: true,
                ShowTitle: false);
        }

        private readonly record struct ResolvedTitle(string Value, bool IsGenerated, bool ShowTitle);
    }
}
