namespace SubscriptionManager.Application.Billing.ProcessWebhook;

public interface IBillingWebhookRepository
{
    Task<PaymentWebhookProcessingResult> ApplyAsync(
        PaymentSubscriptionEvent paymentEvent,
        DateTimeOffset processedAt,
        CancellationToken cancellationToken = default);
}
