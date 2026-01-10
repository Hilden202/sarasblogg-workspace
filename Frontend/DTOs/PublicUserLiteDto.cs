namespace SarasBlogg.DTOs
{
    public class PublicUserLiteDto : IUserNameOnly
    {
        public string Id { get; set; } = "";
        public string UserName { get; set; } = "";
        public IList<string> Roles { get; }
    }
}