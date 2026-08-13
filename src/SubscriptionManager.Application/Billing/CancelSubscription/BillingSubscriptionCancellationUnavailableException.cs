namespace SubscriptionManager.Application.Billing.CancelSubscription;

/// <summary>
/// Indicates that a billing subscription cannot be canceled.
/// </summary>
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
