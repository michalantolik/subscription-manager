namespace SubscriptionManager.Application.Billing.ResumeSubscription;

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
