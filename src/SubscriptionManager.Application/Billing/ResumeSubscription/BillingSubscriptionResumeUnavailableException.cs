namespace SubscriptionManager.Application.Billing.ResumeSubscription;

/// <summary>
/// Indicates that a billing subscription cannot be resumed.
/// </summary>
public sealed class BillingSubscriptionResumeUnavailableException
    : InvalidOperationException
{
    public BillingSubscriptionResumeUnavailableException()
        : base(
            "The billing subscription cannot be resumed.")
    {
    }

    public BillingSubscriptionResumeUnavailableException(
        string message)
        : base(
            message)
    {
    }
}
