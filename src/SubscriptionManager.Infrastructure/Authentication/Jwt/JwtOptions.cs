namespace SubscriptionManager.Infrastructure.Authentication.Jwt;

/// <summary>
/// Configuration options for JWT access tokens.
/// </summary>
public sealed class JwtOptions
{
    public const string SectionName = "Jwt";

    public string Issuer { get; init; } = string.Empty;

    public string Audience { get; init; } = string.Empty;

    public string SigningKey { get; init; } = string.Empty;

    public int ExpirationInMinutes { get; init; }
}
