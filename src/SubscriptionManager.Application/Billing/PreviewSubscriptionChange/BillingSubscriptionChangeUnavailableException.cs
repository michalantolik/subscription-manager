namespace SubscriptionManager.Application.Billing.PreviewSubscriptionChange;

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
