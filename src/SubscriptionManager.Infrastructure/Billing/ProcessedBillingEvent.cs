namespace SubscriptionManager.Infrastructure.Billing;

internal sealed class ProcessedBillingEvent
{
    public string ProviderEventId { get; private set; }

    public DateTimeOffset ProviderEventCreatedAt { get; private set; }

    public DateTimeOffset ProcessedAt { get; private set; }

    private ProcessedBillingEvent()
    {
        ProviderEventId = string.Empty;
    }

    public ProcessedBillingEvent(
        string providerEventId,
        DateTimeOffset providerEventCreatedAt,
        DateTimeOffset processedAt)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(
            providerEventId);

        if (providerEventCreatedAt == default)
        {
            throw new ArgumentException(
                "Provider event creation time is required.",
                nameof(providerEventCreatedAt));
        }

        if (processedAt == default)
        {
            throw new ArgumentException(
                "Processing time is required.",
                nameof(processedAt));
        }

        ProviderEventId = providerEventId;
        ProviderEventCreatedAt =
            providerEventCreatedAt;
        ProcessedAt = processedAt;
    }
}
