namespace SubscriptionManager.Infrastructure.Authentication.Email;

/// <summary>
/// Configuration options for Azure email delivery.
/// </summary>
public sealed class AzureEmailOptions
{
    public const string SectionName = "AzureEmail";

    public string Endpoint { get; init; } = string.Empty;

    public string SenderAddress { get; init; } = string.Empty;
}
