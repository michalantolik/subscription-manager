namespace SubscriptionManager.Application.Billing.ProcessWebhook;

/// <summary>
/// Persistence operations for billing webhook processing.
/// </summary>
public interface IBillingWebhookRepository
{
    Task<PaymentWebhookProcessingResult> ApplyAsync(
        PaymentSubscriptionEvent paymentEvent,
        DateTimeOffset processedAt,
        CancellationToken cancellationToken = default);
}
