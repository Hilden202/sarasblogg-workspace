namespace SarasBloggAPI.DTOs.User
{
    public sealed class AccountDeleteDto
    {
        public string? Password { get; set; }   // null/empty om kontot saknar lokalt lösen
    }
}
