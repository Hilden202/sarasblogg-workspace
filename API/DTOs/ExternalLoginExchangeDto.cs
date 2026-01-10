namespace SarasBloggAPI.DTOs;

/// <summary>
/// Request för att byta external login code mot tokens.
/// </summary>
public sealed class ExternalLoginExchangeDto
{
    public required string Code { get; init; }
}