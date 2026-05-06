using System.Text.Json.Serialization;

namespace SarasBlogg.DTOs
{
    public class CommentCreateRequest
    {
        [JsonPropertyName("bloggId")]
        public int BloggId { get; set; }

        [JsonPropertyName("name")]
        public string? Name { get; set; }

        [JsonPropertyName("content")]
        public string Content { get; set; } = string.Empty;
    }
}
