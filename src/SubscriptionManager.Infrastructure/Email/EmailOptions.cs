namespace SubscriptionManager.Infrastructure.Email;

public sealed class EmailOptions
{
    public const string SectionName = "Email";

    public string ApplicationBaseUrl { get; init; } = string.Empty;
}
