namespace SubscriptionManager.Application.Billing.ProcessWebhook;

/// <summary>
/// Indicates that a payment webhook is invalid.
/// </summary>
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
