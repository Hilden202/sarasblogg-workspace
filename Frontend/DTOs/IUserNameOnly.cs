namespace SarasBlogg.DTOs
{
    public interface IUserNameOnly
    {
        string UserName { get; }
        IList<string> Roles { get; }
    }
}