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
        /// Returns true when <paramref name="title"/> was most likely auto-generated
        /// from the beginning of <paramref name="htmlContent"/> (i.e. the plain-text
        /// content starts with the title text).  Use this to decide whether to render
        /// the title element at all — when true, the first paragraph already acts as
        /// the visible heading so a separate title element would be redundant.
        /// </summary>
        public static bool IsTitleFromContent(string? title, string? htmlContent)
        {
            if (string.IsNullOrWhiteSpace(title) || string.IsNullOrWhiteSpace(htmlContent))
                return false;

            var titleText = title.TrimEnd('…').Trim();
            if (titleText.Length < 10) return false;

            var plain = StripHtml(htmlContent);
            return plain.StartsWith(titleText, StringComparison.OrdinalIgnoreCase);
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
