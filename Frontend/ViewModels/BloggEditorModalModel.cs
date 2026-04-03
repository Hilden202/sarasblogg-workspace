using SarasBlogg.Models;

namespace SarasBlogg.ViewModels
{
    public class BloggEditorModalModel
    {
        public string? EditorAccessToken { get; set; }
        public BloggWithImage? EditedBloggWithImages { get; set; }
        public Blogg NewBlogg { get; set; } = new();
    }
}
