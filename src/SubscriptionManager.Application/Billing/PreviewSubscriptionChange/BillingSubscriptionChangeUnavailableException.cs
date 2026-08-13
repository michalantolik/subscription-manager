namespace SubscriptionManager.Application.Billing.PreviewSubscriptionChange;

/// <summary>
/// Indicates that a billing subscription cannot be changed.
/// </summary>
public sealed class BillingSubscriptionChangeUnavailableException
    : InvalidOperationException
{
    public BillingSubscriptionChangeUnavailableException()
        : base(
            "The billing subscription cannot be changed.")
    {
    }

    public BillingSubscriptionChangeUnavailableException(
        string message)
        : base(
            message)
    {
    }
}
