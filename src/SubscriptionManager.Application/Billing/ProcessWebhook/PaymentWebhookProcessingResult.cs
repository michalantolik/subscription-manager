namespace SubscriptionManager.Application.Billing.ProcessWebhook;

public enum PaymentWebhookProcessingResult
{
    Applied = 1,
    Duplicate = 2,
    Stale = 3,
    Ignored = 4
}
