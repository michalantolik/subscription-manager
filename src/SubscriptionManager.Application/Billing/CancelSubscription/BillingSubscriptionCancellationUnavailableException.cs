namespace SubscriptionManager.Application.Billing.CancelSubscription;

public sealed class BillingSubscriptionCancellationUnavailableException
    : InvalidOperationException
{
    public BillingSubscriptionCancellationUnavailableException()
        : base(
            "The billing subscription cannot be canceled.")
    {
    }

    public BillingSubscriptionCancellationUnavailableException(
        string message)
        : base(
            message)
    {
    }
}
