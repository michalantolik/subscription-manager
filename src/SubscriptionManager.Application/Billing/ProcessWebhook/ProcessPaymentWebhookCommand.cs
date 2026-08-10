namespace SubscriptionManager.Application.Billing.ProcessWebhook;

public sealed record ProcessPaymentWebhookCommand(
    string Payload,
    string Signature);
