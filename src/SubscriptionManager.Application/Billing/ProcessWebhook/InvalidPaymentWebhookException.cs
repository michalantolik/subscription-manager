namespace SubscriptionManager.Application.Billing.ProcessWebhook;

public sealed class InvalidPaymentWebhookException
    : Exception
{
    public InvalidPaymentWebhookException(
        string message)
        : base(message)
    {
    }

    public InvalidPaymentWebhookException(
        string message,
        Exception innerException)
        : base(
            message,
            innerException)
    {
    }
}
