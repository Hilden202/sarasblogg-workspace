using System.Text.RegularExpressions;

namespace SarasBloggAPI.Services.Comment;

public static class ForbiddenWordPatternNormalizer
{
    private static readonly IReadOnlyDictionary<char, string> CharacterMap = new Dictionary<char, string>
    {
        ['a'] = "[a4@]",
        ['o'] = "[o0]",
        ['e'] = "[e3]",
        ['i'] = "[i1|!]",
        ['u'] = "[u\u00FCv]",
        ['c'] = "[ck]",
        ['s'] = "[s$5]",
        ['g'] = "[g9]"
    };

    private static readonly Regex RegexTokenPattern = new(
        @"[\\\[\]\(\)\{\}\|\^\$\*\+\?]",
        RegexOptions.Compiled);

    public static string Normalize(string? wordOrPattern)
    {
        if (string.IsNullOrWhiteSpace(wordOrPattern))
            return string.Empty;

        var trimmed = wordOrPattern.Trim();
        if (LooksLikeRegexPattern(trimmed) && IsValidRegex(trimmed))
            return trimmed;

        return ToRegexPattern(trimmed);
    }

    private static bool LooksLikeRegexPattern(string value)
        => RegexTokenPattern.IsMatch(value);

    private static bool IsValidRegex(string pattern)
    {
        try
        {
            _ = new Regex(pattern);
            return true;
        }
        catch
        {
            return false;
        }
    }

    private static string ToRegexPattern(string word)
    {
        return string.Concat(word.ToLowerInvariant().Select(c =>
            CharacterMap.TryGetValue(c, out var replacement)
                ? replacement
                : Regex.Escape(c.ToString())));
    }
}
