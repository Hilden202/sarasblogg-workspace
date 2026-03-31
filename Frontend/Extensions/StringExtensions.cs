using System.Net;
using System.Text;
using System.Text.RegularExpressions;

namespace SarasBlogg.Extensions
{
    public static class StringExtensions
    {
        private static readonly Regex HtmlTagRegex = new("<[^>]+>", RegexOptions.Compiled);
        private static readonly Regex ScriptAndStyleRegex = new("<(script|style)\\b[^>]*>.*?</\\1>", RegexOptions.IgnoreCase | RegexOptions.Singleline | RegexOptions.Compiled);

        public static string LimitLength(this string? str, int maxLength, bool stripHtml = false)
        {
            if (string.IsNullOrEmpty(str))
                return string.Empty;

            // Bygg säker, begränsad preview-HTML för korten.
            if (stripHtml)
            {
                str = BuildSafePreviewHtml(str);
            }

            // Korta texten om den är för lång
            if (str.Length <= maxLength)
                return str;

            return TruncateHtmlPreservingBold(str, maxLength) + "...";
        }

        private static string BuildSafePreviewHtml(string html)
        {
            html = ScriptAndStyleRegex.Replace(html, string.Empty);

            // 🔥 FIX: Ta bort TinyMCE "fake whitespace"
            html = html.Replace("&nbsp;", " ");
            html = Regex.Replace(html, @"\s*\u00A0\s*", " "); // extra säkerhet (unicode nbsp)

            var sb = new StringBuilder();
            var lastIndex = 0;
            var strongDepth = 0;

            foreach (Match match in HtmlTagRegex.Matches(html))
            {
                if (match.Index > lastIndex)
                {
                    var text = html[lastIndex..match.Index];
                    if (!string.IsNullOrEmpty(text))
                        sb.Append(WebUtility.HtmlDecode(text));
                }

                var rawTag = match.Value;
                var tag = rawTag.Trim('<', '>', ' ', '\t', '\r', '\n');
                var isClosing = tag.StartsWith('/');
                var tagName = ExtractTagName(tag, isClosing);
                if (string.IsNullOrEmpty(tagName))
                {
                    lastIndex = match.Index + match.Length;
                    continue;
                }

                switch (tagName)
                {
                    case "p" when isClosing:
                        sb.Append("<br><br>");
                        break;
                    case "br":
                        sb.Append("<br>");
                        break;
                    case "strong":
                    case "b":
                        if (isClosing)
                        {
                            if (strongDepth > 0)
                            {
                                sb.Append("</strong>");
                                strongDepth--;
                            }
                        }
                        else
                        {
                            sb.Append("<strong>");
                            strongDepth++;
                        }
                        break;
                }

                lastIndex = match.Index + match.Length;
            }

            if (lastIndex < html.Length)
            {
                sb.Append(WebUtility.HtmlDecode(html[lastIndex..]));
            }

            while (strongDepth-- > 0)
                sb.Append("</strong>");

            return sb.ToString().TrimEnd();
        }

        private static string TruncateHtmlPreservingBold(string html, int maxLength)
        {
            var sb = new StringBuilder();
            var visibleChars = 0;
            var lastIndex = 0;
            var strongDepth = 0;

            foreach (Match match in HtmlTagRegex.Matches(html))
            {
                if (match.Index > lastIndex)
                {
                    var text = html[lastIndex..match.Index];
                    foreach (var ch in text)
                    {
                        if (visibleChars >= maxLength)
                            goto Done;

                        sb.Append(ch);
                        visibleChars++;
                    }
                }

                var rawTag = match.Value;
                var tag = rawTag.Trim('<', '>', ' ', '\t', '\r', '\n');
                var isClosing = tag.StartsWith('/');
                var tagName = ExtractTagName(tag, isClosing);
                if (string.IsNullOrEmpty(tagName))
                {
                    lastIndex = match.Index + match.Length;
                    continue;
                }

                if (tagName is "strong")
                {
                    if (isClosing)
                    {
                        if (strongDepth > 0)
                        {
                            sb.Append("</strong>");
                            strongDepth--;
                        }
                    }
                    else
                    {
                        sb.Append("<strong>");
                        strongDepth++;
                    }
                }
                else if (tagName is "br")
                {
                    sb.Append("<br>");
                }

                lastIndex = match.Index + match.Length;
            }

            if (lastIndex < html.Length)
            {
                foreach (var ch in html[lastIndex..])
                {
                    if (visibleChars >= maxLength)
                        break;

                    sb.Append(ch);
                    visibleChars++;
                }
            }

        Done:
            while (strongDepth-- > 0)
                sb.Append("</strong>");

            return sb.ToString();
        }

        private static string ExtractTagName(string tag, bool isClosing)
        {
            var tagBody = isClosing ? tag[1..] : tag;
            var parts = tagBody.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            return parts.Length == 0 ? string.Empty : parts[0].ToLowerInvariant();
        }
    }
}
