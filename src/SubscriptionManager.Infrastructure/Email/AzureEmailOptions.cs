namespace SubscriptionManager.Infrastructure.Email;

public sealed class AzureEmailOptions
{
    public const string SectionName = "AzureEmail";

    public string Endpoint { get; init; } = string.Empty;

    public string SenderAddress { get; init; } = string.Empty;
}
