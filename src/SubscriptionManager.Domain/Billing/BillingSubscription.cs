namespace SubscriptionManager.Domain.Billing;

public sealed class BillingSubscription
{
    public Guid Id { get; private set; }

    public Guid UserId { get; private set; }

    public SubscriptionPlan Plan { get; private set; }

    public BillingInterval BillingInterval { get; private set; }

    public BillingSubscriptionStatus Status { get; private set; }

    public DateTimeOffset CurrentPeriodStart { get; private set; }

    public DateTimeOffset CurrentPeriodEnd { get; private set; }

    public bool CancelAtPeriodEnd { get; private set; }

    private BillingSubscription()
    {
    }

    public BillingSubscription(
        Guid id,
        Guid userId,
        SubscriptionPlan plan,
        BillingInterval billingInterval,
        DateTimeOffset currentPeriodStart,
        DateTimeOffset currentPeriodEnd)
    {
        if (id == Guid.Empty)
        {
            throw new ArgumentException(
                "Billing subscription ID is required.",
                nameof(id));
        }

        if (userId == Guid.Empty)
        {
            throw new ArgumentException(
                "User ID is required.",
                nameof(userId));
        }

        if (currentPeriodEnd <= currentPeriodStart)
        {
            throw new ArgumentException(
                "Current period end must be after its start.",
                nameof(currentPeriodEnd));
        }

        Id = id;
        UserId = userId;
        Plan = plan;
        BillingInterval = billingInterval;
        Status = BillingSubscriptionStatus.Active;
        CurrentPeriodStart = currentPeriodStart;
        CurrentPeriodEnd = currentPeriodEnd;
    }

    public void Cancel()
    {
        Status = BillingSubscriptionStatus.Canceled;
        CancelAtPeriodEnd = true;
    }
}
