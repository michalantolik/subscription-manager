namespace SubscriptionManager.Infrastructure.Authentication.Email;

/// <summary>
/// Configuration options for account email links.
/// </summary>
public sealed class EmailOptions
{
    public const string SectionName = "Email";

    public string ApplicationBaseUrl { get; init; } = string.Empty;
}
