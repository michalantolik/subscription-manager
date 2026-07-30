namespace SubscriptionManager.Blazor.Configuration;

public sealed class AuthenticationCookieOptions
{
    public const string SectionName = "Authentication";

    /// <summary>
    /// Lifetime of the authentication cookie.
    /// This value should match Jwt:ExpirationInMinutes
    /// configured in the API.
    /// </summary>
    public int AuthenticationCookieExpirationInMinutes { get; init; }
}
