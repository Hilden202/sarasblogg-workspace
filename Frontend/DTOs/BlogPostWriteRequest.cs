using System.Text.Json.Serialization;

namespace SarasBlogg.DTOs
{
    public class BlogPostWriteRequest
    {
        [JsonPropertyName("title")]
        public string? Title { get; set; }

        [JsonPropertyName("content")]
        public string Content { get; set; } = string.Empty;

        [JsonPropertyName("author")]
        public string? Author { get; set; }

        [JsonPropertyName("launchDateLocal")]
        public DateTime? LaunchDateLocal { get; set; }

        [JsonPropertyName("hidden")]
        public bool Hidden { get; set; }

        [JsonPropertyName("isArchived")]
        public bool IsArchived { get; set; }
    }
}
