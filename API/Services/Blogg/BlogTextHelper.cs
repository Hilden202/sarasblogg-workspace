using System.Net;
using System.Text.RegularExpressions;

namespace SarasBloggAPI.Services.Blogg;

public static class BlogTextHelper
{
    public static string StripHtml(string? html)
    {
        if (string.IsNullOrWhiteSpace(html))
            return string.Empty;

        var text = Regex.Replace(html, "<[^>]+>", " ");
        text = WebUtility.HtmlDecode(text);
        return Regex.Replace(text, "\\s+", " ").Trim();
    }

    public static string GenerateFallbackTitle(string? content, int maxLength = 80)
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

    public static int CalculateReadingTimeMinutes(string? content, int wordsPerMinute = 220)
    {
        var text = StripHtml(content);
        if (string.IsNullOrWhiteSpace(text))
            return 1;

        var words = Regex.Matches(text, "\\S+").Count;
        return Math.Max(1, (int)Math.Ceiling(words / (double)wordsPerMinute));
    }

    public static bool IsTitleGeneratedFromContent(string? title, string? htmlContent)
    {
        if (string.IsNullOrWhiteSpace(title) || string.IsNullOrWhiteSpace(htmlContent))
            return false;

        var titleNorm = Normalize(StripHtml(RemoveTrailingEllipsis(title)));
        if (titleNorm.Length < 5)
            return false;

        var contentNorm = Normalize(StripHtml(htmlContent));

        return contentNorm.StartsWith(titleNorm, StringComparison.Ordinal) ||
               (contentNorm.Length >= titleNorm.Length &&
                contentNorm[..titleNorm.Length].Equals(titleNorm, StringComparison.Ordinal));
    }

    private static string RemoveTrailingEllipsis(string value)
    {
        var trimmed = value.Trim();
        if (trimmed.EndsWith("...", StringComparison.Ordinal))
            return trimmed[..^3];

        if (trimmed.EndsWith('\u2026'))
            return trimmed[..^1];

        return trimmed;
    }

    private static string Normalize(string text)
        => Regex.Replace(text.ToLowerInvariant(), "\\s+", " ").Trim();
}
