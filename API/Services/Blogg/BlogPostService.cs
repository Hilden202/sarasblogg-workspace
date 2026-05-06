using System.Net;
using System.Security.Claims;
using System.Text.RegularExpressions;
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
            var blogg = new BloggModel
            {
                Title = ResolveTitle(request.Title, sanitizedContent),
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

            existing.Title = ResolveTitle(request.Title, sanitizedContent);
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

            var legacyLaunchDate = request.GetLegacyLaunchDate();
            if (legacyLaunchDate.HasValue)
                return NormalizeLegacyUtc(legacyLaunchDate.Value);

            return DateTime.UtcNow;
        }

        private static DateTime NormalizeLegacyUtc(DateTime value)
        {
            return value.Kind switch
            {
                DateTimeKind.Utc => value,
                DateTimeKind.Local => value.ToUniversalTime(),
                _ => DateTime.SpecifyKind(value, DateTimeKind.Utc)
            };
        }

        private static string ResolveTitle(string? title, string content)
        {
            if (!string.IsNullOrWhiteSpace(title))
                return title.Trim();

            return GenerateFallbackTitle(content);
        }

        private static string GenerateFallbackTitle(string? content, int maxLength = 80)
        {
            var plain = StripHtml(content);
            if (string.IsNullOrWhiteSpace(plain))
                return string.Empty;

            var text = plain.Trim();

            if (text.Length <= maxLength)
                return text;

            var truncated = text.Substring(0, maxLength);

            var lastSpace = truncated.LastIndexOf(' ');
            if (lastSpace > 20)
            {
                truncated = truncated.Substring(0, lastSpace);
            }

            return truncated.TrimEnd() + "...";
        }

        private static string StripHtml(string? html)
        {
            if (string.IsNullOrWhiteSpace(html))
                return string.Empty;

            var text = Regex.Replace(html, "<[^>]+>", " ");
            text = WebUtility.HtmlDecode(text);
            return Regex.Replace(text, "\\s+", " ").Trim();
        }
    }
}
