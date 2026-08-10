namespace SubscriptionManager.Domain.Billing;

public enum BillingSubscriptionStatus
{
    Incomplete = 1,
    IncompleteExpired = 2,
    Trialing = 3,
    Active = 4,
    PastDue = 5,
    Canceled = 6,
    Unpaid = 7,
    Paused = 8
}
