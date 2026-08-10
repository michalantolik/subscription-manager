namespace SubscriptionManager.Domain.Billing;

public sealed class BillingSubscription
{
    public Guid Id { get; private set; }

    public Guid UserId { get; private set; }

    public SubscriptionPlan Plan { get; private set; }

    public BillingInterval BillingInterval { get; private set; }

    public BillingSubscriptionStatus Status { get; private set; }

    public string? ProviderCustomerId { get; private set; }

    public string? ProviderSubscriptionId { get; private set; }

    public string? ProviderPriceId { get; private set; }

    public DateTimeOffset CurrentPeriodStart { get; private set; }

    public DateTimeOffset CurrentPeriodEnd { get; private set; }

    public DateTimeOffset? LastProviderEventCreatedAt { get; private set; }

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

        ValidatePlan(
            plan);

        ValidateBillingInterval(
            billingInterval);

        ValidatePeriod(
            currentPeriodStart,
            currentPeriodEnd);

        Id = id;
        UserId = userId;
        Plan = plan;
        BillingInterval = billingInterval;
        Status = BillingSubscriptionStatus.Active;
        CurrentPeriodStart = currentPeriodStart;
        CurrentPeriodEnd = currentPeriodEnd;
    }

    public void LinkToPaymentProvider(
        string customerId,
        string subscriptionId,
        string priceId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(
            customerId);

        ArgumentException.ThrowIfNullOrWhiteSpace(
            subscriptionId);

        ArgumentException.ThrowIfNullOrWhiteSpace(
            priceId);

        ProviderCustomerId = customerId;
        ProviderSubscriptionId = subscriptionId;
        ProviderPriceId = priceId;
    }

    public void Synchronize(
        SubscriptionPlan plan,
        BillingInterval billingInterval,
        BillingSubscriptionStatus status,
        string priceId,
        DateTimeOffset currentPeriodStart,
        DateTimeOffset currentPeriodEnd,
        bool cancelAtPeriodEnd)
    {
        ValidateSynchronization(
            plan,
            billingInterval,
            status,
            priceId,
            currentPeriodStart,
            currentPeriodEnd);

        ApplySynchronization(
            plan,
            billingInterval,
            status,
            priceId,
            currentPeriodStart,
            currentPeriodEnd,
            cancelAtPeriodEnd);
    }

    public bool ApplyProviderEvent(
        DateTimeOffset providerEventCreatedAt,
        SubscriptionPlan plan,
        BillingInterval billingInterval,
        BillingSubscriptionStatus status,
        string priceId,
        DateTimeOffset currentPeriodStart,
        DateTimeOffset currentPeriodEnd,
        bool cancelAtPeriodEnd)
    {
        if (providerEventCreatedAt == default)
        {
            throw new ArgumentException(
                "Provider event creation time is required.",
                nameof(providerEventCreatedAt));
        }

        ValidateSynchronization(
            plan,
            billingInterval,
            status,
            priceId,
            currentPeriodStart,
            currentPeriodEnd);

        if (LastProviderEventCreatedAt >=
            providerEventCreatedAt)
        {
            return false;
        }

        ApplySynchronization(
            plan,
            billingInterval,
            status,
            priceId,
            currentPeriodStart,
            currentPeriodEnd,
            cancelAtPeriodEnd);

        LastProviderEventCreatedAt =
            providerEventCreatedAt;

        return true;
    }

    public void ScheduleCancellation()
    {
        if (Status is BillingSubscriptionStatus.Canceled or
            BillingSubscriptionStatus.IncompleteExpired)
        {
            throw new InvalidOperationException(
                "An ended billing subscription cannot be canceled again.");
        }

        CancelAtPeriodEnd = true;
    }

    private void ApplySynchronization(
        SubscriptionPlan plan,
        BillingInterval billingInterval,
        BillingSubscriptionStatus status,
        string priceId,
        DateTimeOffset currentPeriodStart,
        DateTimeOffset currentPeriodEnd,
        bool cancelAtPeriodEnd)
    {
        Plan = plan;
        BillingInterval = billingInterval;
        Status = status;
        ProviderPriceId = priceId;
        CurrentPeriodStart = currentPeriodStart;
        CurrentPeriodEnd = currentPeriodEnd;
        CancelAtPeriodEnd = cancelAtPeriodEnd;
    }

    private static void ValidateSynchronization(
        SubscriptionPlan plan,
        BillingInterval billingInterval,
        BillingSubscriptionStatus status,
        string priceId,
        DateTimeOffset currentPeriodStart,
        DateTimeOffset currentPeriodEnd)
    {
        ValidatePlan(
            plan);

        ValidateBillingInterval(
            billingInterval);

        if (!Enum.IsDefined(status))
        {
            throw new ArgumentOutOfRangeException(
                nameof(status),
                status,
                "The billing subscription status is not supported.");
        }

        ArgumentException.ThrowIfNullOrWhiteSpace(
            priceId);

        ValidatePeriod(
            currentPeriodStart,
            currentPeriodEnd);
    }

    private static void ValidatePlan(
        SubscriptionPlan plan)
    {
        if (!Enum.IsDefined(plan) ||
            plan == SubscriptionPlan.Free)
        {
            throw new ArgumentException(
                "Billing subscription requires a paid plan.",
                nameof(plan));
        }
    }

    private static void ValidateBillingInterval(
        BillingInterval billingInterval)
    {
        if (!Enum.IsDefined(billingInterval))
        {
            throw new ArgumentOutOfRangeException(
                nameof(billingInterval),
                billingInterval,
                "The billing interval is not supported.");
        }
    }

    private static void ValidatePeriod(
        DateTimeOffset currentPeriodStart,
        DateTimeOffset currentPeriodEnd)
    {
        if (currentPeriodEnd <= currentPeriodStart)
        {
            throw new ArgumentException(
                "Current period end must be after its start.",
                nameof(currentPeriodEnd));
        }
    }
}
