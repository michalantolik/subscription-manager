namespace SubscriptionManager.Application.Billing.ProcessWebhook;

/// <summary>
/// Request to process a payment webhook.
/// </summary>
public sealed record ProcessPaymentWebhookCommand(
    string Payload,
    string Signature);
