using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace SarasBloggAPI.DTOs.Blogg
{
    [JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
    public class BlogPostWriteRequest
    {
        public string? Title { get; set; }
        public bool? ShowTitle { get; set; }

        [Required]
        public string? Content { get; set; }

        public string? Author { get; set; }
        public DateTime? LaunchDateLocal { get; set; }
        public bool Hidden { get; set; }
        public bool IsArchived { get; set; }
    }
}
