using System.Net;
using System.Text.RegularExpressions;

namespace SarasBlogg.Helpers
{
    public static class BloggTextHelper
    {
        public static string StripHtml(string? html)
        {
            if (string.IsNullOrWhiteSpace(html)) return "";
            var text = Regex.Replace(html, "<[^>]+>", " ");
            text = WebUtility.HtmlDecode(text);
            return Regex.Replace(text, @"\s+", " ").Trim();
        }

        public static string GenerateFallbackTitle(string? content, int maxLength = 60)
        {
            var plain = StripHtml(content);
            if (string.IsNullOrWhiteSpace(plain)) return "";
            return plain.Length > maxLength
                ? plain[..maxLength].TrimEnd() + "…"
                : plain;
        }

        /// <summary>
        /// Normalizes a plain-text string for comparison: lowercase, collapse whitespace.
        /// </summary>
        private static string Normalize(string text)
            => Regex.Replace(text.ToLowerInvariant(), @"\s+", " ").Trim();

        /// <summary>
        /// Returns true when <paramref name="title"/> was most likely auto-generated
        /// from the beginning of <paramref name="htmlContent"/>.  Both strings are
        /// stripped of HTML, entity-decoded, lowercased, and whitespace-normalized
        /// before comparison.  The title may end with a truncation ellipsis (…), in
        /// which case the leading <c>N</c> characters of the content are compared
        /// against the title (without the ellipsis), so minor formatting differences
        /// and mid-word truncations are handled correctly.
        /// </summary>
        public static bool IsTitleFromContent(string? title, string? htmlContent)
        {
            if (string.IsNullOrWhiteSpace(title) || string.IsNullOrWhiteSpace(htmlContent))
                return false;

            // Strip the trailing ellipsis that GenerateFallbackTitle appends when truncating.
            var titleNorm = Normalize(StripHtml(title).TrimEnd('…'));
            if (titleNorm.Length < 5) return false;

            var contentNorm = Normalize(StripHtml(htmlContent));

            // If content starts with the title verbatim (non-truncated titles).
            if (contentNorm.StartsWith(titleNorm, StringComparison.Ordinal))
                return true;

            // Truncated titles: compare the first titleNorm.Length characters of content.
            if (contentNorm.Length >= titleNorm.Length &&
                contentNorm[..titleNorm.Length].Equals(titleNorm, StringComparison.Ordinal))
                return true;

            return false;
        }

        /// <summary>
        /// Returns the HTML content with the first block element removed when its
        /// plain text matches the start of <paramref name="title"/>.  This prevents
        /// the title and the opening line of the preview from showing identical text.
        /// </summary>
        public static string RemoveLeadingDuplicateContent(string? title, string? htmlContent)
        {
            if (string.IsNullOrWhiteSpace(title) || string.IsNullOrWhiteSpace(htmlContent))
                return htmlContent ?? "";

            var titleText = title.TrimEnd('…').Trim();
            if (titleText.Length < 10) return htmlContent;

            var match = Regex.Match(
                htmlContent,
                @"^(\s*<(?:p|h[1-6]|div|blockquote)[^>]*>)(.*?)(</(?:p|h[1-6]|div|blockquote)>)",
                RegexOptions.IgnoreCase | RegexOptions.Singleline);

            if (!match.Success) return htmlContent;

            var blockPlain = StripHtml(match.Groups[2].Value);

            if (blockPlain.StartsWith(titleText, StringComparison.OrdinalIgnoreCase) ||
                titleText.StartsWith(blockPlain, StringComparison.OrdinalIgnoreCase))
            {
                return htmlContent[(match.Index + match.Length)..].TrimStart();
            }

            return htmlContent;
        }
    }
}
