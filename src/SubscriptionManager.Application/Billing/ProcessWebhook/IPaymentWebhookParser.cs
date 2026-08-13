namespace SubscriptionManager.Application.Billing.ProcessWebhook;

/// <summary>
/// Parses payment provider webhooks into billing subscription events.
/// </summary>
public interface IPaymentWebhookParser
{
    PaymentSubscriptionEvent? Parse(
        string payload,
        string signature);
}
