namespace SubscriptionManager.Application.Billing.ProcessWebhook;

/// <summary>
/// Defines the result of payment webhook processing.
/// </summary>
public enum PaymentWebhookProcessingResult
{
    Applied = 1,
    Duplicate = 2,
    Stale = 3,
    Ignored = 4
}
