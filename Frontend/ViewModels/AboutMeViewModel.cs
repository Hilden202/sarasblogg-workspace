using SarasBlogg.Models;

namespace SarasBlogg.ViewModels
{
    public class AboutMeViewModel
    {
        public AboutMe Data { get; init; } = new();

        public bool HasStructuredInfo =>
            !string.IsNullOrWhiteSpace(Data.Name) ||
            !string.IsNullOrWhiteSpace(Data.City) ||
            Data.Age.HasValue ||
            !string.IsNullOrWhiteSpace(Data.Family);

        public static AboutMeViewModel From(AboutMe aboutMe) => new()
        {
            Data = aboutMe
        };
    }
}
