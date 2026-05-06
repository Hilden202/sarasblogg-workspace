using System.Text.Json;
using System.Text.Json.Serialization;

namespace SarasBloggAPI.DTOs.Blogg
{
    public class BlogPostWriteRequest
    {
        public string? Title { get; set; }
        public string Content { get; set; } = string.Empty;
        public string? Author { get; set; }
        public DateTime? LaunchDateLocal { get; set; }
        public bool Hidden { get; set; }
        public bool IsArchived { get; set; }

        [JsonExtensionData]
        public Dictionary<string, JsonElement>? ExtensionData { get; set; }

        public int? GetLegacyId()
        {
            return TryGetExtension("id", out var value) && value.ValueKind == JsonValueKind.Number
                && value.TryGetInt32(out var id)
                    ? id
                    : null;
        }

        public DateTime? GetLegacyLaunchDate()
        {
            if (!TryGetExtension("launchDate", out var value))
                return null;

            if (value.ValueKind == JsonValueKind.String && value.TryGetDateTime(out var date))
                return date;

            return null;
        }

        private bool TryGetExtension(string name, out JsonElement value)
        {
            value = default;

            if (ExtensionData == null)
                return false;

            foreach (var item in ExtensionData)
            {
                if (string.Equals(item.Key, name, StringComparison.OrdinalIgnoreCase))
                {
                    value = item.Value;
                    return true;
                }
            }

            return false;
        }
    }
}
