using System.Globalization;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;

namespace SarasBlogg.Extensions
{
    public static class StringExtensions
    {
        private static readonly Regex HtmlTagRegex = new("<[^>]+>", RegexOptions.Compiled);
        private static readonly Regex ScriptAndStyleRegex = new("<(script|style)\\b[^>]*>.*?</\\1>", RegexOptions.IgnoreCase | RegexOptions.Singleline | RegexOptions.Compiled);
        private static readonly Regex StyleAttributeRegex = new("style\\s*=\\s*(\"(?<value>[^\"]*)\"|'(?<value>[^']*)'|(?<value>[^\\s>]+))", RegexOptions.IgnoreCase | RegexOptions.Compiled);
        private const double MaxPreviewFontSizePx = 18d;
        private const double MaxPreviewFontSizeEm = 1.2d;
        private const int MaxStyleAttributeLength = 300;

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

            return TruncateHtmlPreservingAllowedTags(str, maxLength) + "...";
        }

        private static string BuildSafePreviewHtml(string html)
        {
            html = ScriptAndStyleRegex.Replace(html, string.Empty);

            // 🔥 FIX: Ta bort TinyMCE "fake whitespace"
            html = html.Replace("&nbsp;", " ");
            html = Regex.Replace(html, @"\s*\u00A0\s*", " "); // extra säkerhet (unicode nbsp)

            var sb = new StringBuilder();
            var lastIndex = 0;
            var openTags = new Stack<string>();

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
                            TryCloseTag("strong", openTags, sb);
                        }
                        else
                        {
                            sb.Append("<strong>");
                            openTags.Push("strong");
                        }
                        break;
                    case "em":
                    case "i":
                        if (isClosing)
                        {
                            TryCloseTag("em", openTags, sb);
                        }
                        else
                        {
                            sb.Append("<em>");
                            openTags.Push("em");
                        }
                        break;
                    case "u":
                        if (isClosing)
                        {
                            TryCloseTag("u", openTags, sb);
                        }
                        else
                        {
                            sb.Append("<u>");
                            openTags.Push("u");
                        }
                        break;
                    case "span":
                        if (isClosing)
                        {
                            TryCloseTag("span", openTags, sb);
                        }
                        else
                        {
                            var style = SanitizeSpanStyle(rawTag);
                            if (!string.IsNullOrEmpty(style))
                            {
                                sb.Append("<span style=\"");
                                sb.Append(WebUtility.HtmlEncode(style));
                                sb.Append("\">");
                                openTags.Push("span");
                            }
                        }
                        break;
                }

                lastIndex = match.Index + match.Length;
            }

            if (lastIndex < html.Length)
            {
                sb.Append(WebUtility.HtmlDecode(html[lastIndex..]));
            }

            while (openTags.Count > 0)
                sb.Append("</").Append(openTags.Pop()).Append('>');

            return sb.ToString().TrimEnd();
        }

        private static string TruncateHtmlPreservingAllowedTags(string html, int maxLength)
        {
            var sb = new StringBuilder();
            var visibleChars = 0;
            var lastIndex = 0;
            var openTags = new Stack<string>();

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

                if (tagName is "strong" or "em" or "u" or "span")
                {
                    if (isClosing)
                    {
                        TryCloseTag(tagName, openTags, sb);
                    }
                    else
                    {
                        if (tagName == "span")
                        {
                            var style = SanitizeSpanStyle(rawTag);
                            if (string.IsNullOrEmpty(style))
                            {
                                lastIndex = match.Index + match.Length;
                                continue;
                            }

                            sb.Append("<span style=\"");
                            sb.Append(WebUtility.HtmlEncode(style));
                            sb.Append("\">");
                        }
                        else
                        {
                            sb.Append('<').Append(tagName).Append('>');
                        }

                        openTags.Push(tagName);
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
            while (openTags.Count > 0)
                sb.Append("</").Append(openTags.Pop()).Append('>');

            return sb.ToString();
        }

        private static bool TryCloseTag(string tagName, Stack<string> openTags, StringBuilder sb)
        {
            if (openTags.Count == 0 || openTags.Peek() != tagName)
                return false;

            openTags.Pop();
            sb.Append("</").Append(tagName).Append('>');
            return true;
        }

        private static string SanitizeSpanStyle(string rawTag)
        {
            var styleMatch = StyleAttributeRegex.Match(rawTag);
            if (!styleMatch.Success)
                return string.Empty;

            var styleValue = WebUtility.HtmlDecode(styleMatch.Groups["value"].Value).Trim();
            if (string.IsNullOrEmpty(styleValue) || styleValue.Length > MaxStyleAttributeLength)
                return string.Empty;

            var safeRules = new List<string>(capacity: 3);

            foreach (var declaration in styleValue.Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
            {
                var separatorIndex = declaration.IndexOf(':');
                if (separatorIndex <= 0)
                    continue;

                var property = declaration[..separatorIndex].Trim().ToLowerInvariant();
                var value = declaration[(separatorIndex + 1)..].Trim();

                if (!IsSafeCssValue(value))
                    continue;

                switch (property)
                {
                    case "color":
                        safeRules.Add($"color: {value}");
                        break;
                    case "font-weight":
                        if (IsSafeFontWeight(value))
                            safeRules.Add($"font-weight: {value}");
                        break;
                    case "font-size":
                        if (TryClampFontSize(value, out var clampedFontSize))
                            safeRules.Add($"font-size: {clampedFontSize}");
                        break;
                }
            }

            return string.Join("; ", safeRules);
        }

        private static bool TryClampFontSize(string value, out string clampedValue)
        {
            clampedValue = string.Empty;
            var normalized = value.Trim().ToLowerInvariant();
            if (normalized.EndsWith("px") && double.TryParse(normalized[..^2], NumberStyles.Float, CultureInfo.InvariantCulture, out var px))
            {
                clampedValue = $"{Math.Min(px, MaxPreviewFontSizePx).ToString("0.##", CultureInfo.InvariantCulture)}px";
                return true;
            }

            if (normalized.EndsWith("em") && double.TryParse(normalized[..^2], NumberStyles.Float, CultureInfo.InvariantCulture, out var em))
            {
                clampedValue = $"{Math.Min(em, MaxPreviewFontSizeEm).ToString("0.##", CultureInfo.InvariantCulture)}em";
                return true;
            }

            return false;
        }

        private static bool IsSafeCssValue(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return false;

            var normalized = value.ToLowerInvariant();
            return !normalized.Contains("url(")
                   && !normalized.Contains("expression(")
                   && !normalized.Contains("javascript:")
                   && !normalized.Contains("@import")
                   && !normalized.Contains("var(");
        }

        private static bool IsSafeFontWeight(string value)
        {
            var normalized = value.Trim().ToLowerInvariant();
            return normalized is "normal" or "bold" or "bolder" or "lighter"
                   || (int.TryParse(normalized, out var weight) && weight >= 100 && weight <= 900 && weight % 100 == 0);
        }

        private static string ExtractTagName(string tag, bool isClosing)
        {
            var tagBody = isClosing ? tag[1..] : tag;
            var parts = tagBody.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            return parts.Length == 0 ? string.Empty : parts[0].ToLowerInvariant();
        }
    }
}
