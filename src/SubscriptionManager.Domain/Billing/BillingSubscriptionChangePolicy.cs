namespace SubscriptionManager.Domain.Billing;

public static class BillingSubscriptionChangePolicy
{
    public static BillingSubscriptionChangeTiming GetTiming(
        SubscriptionPlan currentPlan,
        BillingInterval currentBillingInterval,
        SubscriptionPlan targetPlan,
        BillingInterval targetBillingInterval)
    {
        ValidatePaidPlan(
            currentPlan,
            nameof(currentPlan));

        ValidatePaidPlan(
            targetPlan,
            nameof(targetPlan));

        ValidateBillingInterval(
            currentBillingInterval,
            nameof(currentBillingInterval));

        ValidateBillingInterval(
            targetBillingInterval,
            nameof(targetBillingInterval));

        if (currentPlan == targetPlan &&
            currentBillingInterval == targetBillingInterval)
        {
            throw new InvalidOperationException(
                "The selected subscription plan and billing interval are already active.");
        }

        if (currentBillingInterval !=
            targetBillingInterval)
        {
            return BillingSubscriptionChangeTiming
                .NextBillingPeriod;
        }

        return targetPlan > currentPlan
            ? BillingSubscriptionChangeTiming.Immediate
            : BillingSubscriptionChangeTiming
                .NextBillingPeriod;
    }

    private static void ValidatePaidPlan(
        SubscriptionPlan plan,
        string parameterName)
    {
        if (!Enum.IsDefined(plan) ||
            plan == SubscriptionPlan.Free)
        {
            throw new ArgumentOutOfRangeException(
                parameterName,
                plan,
                "A subscription change requires a paid plan.");
        }
    }

    private static void ValidateBillingInterval(
        BillingInterval billingInterval,
        string parameterName)
    {
        if (!Enum.IsDefined(billingInterval))
        {
            throw new ArgumentOutOfRangeException(
                parameterName,
                billingInterval,
                "The billing interval is not supported.");
        }
    }
}
