namespace SarasBloggAPI.DTOs;

// 🔐 Kortlivad payload för external login
// Ligger endast i serverns MemoryCache och nås via engångskod
public sealed class ExternalLoginCodeDto
{
    public required string AccessToken { get; init; }
    public required string RefreshToken { get; init; }
    public required DateTime AccessTokenExpiresUtc { get; init; }
}