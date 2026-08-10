using SubscriptionManager.Domain.Billing;

namespace SubscriptionManager.Application.Common.Identity;

public static class SubscriptionPlanLimits
{
    public const int FreeSubscriptionLimit = 5;

    public const int PlusDailySavingsPlanLimit = 5;

    public const int PremiumDailySavingsPlanLimit = 20;

    public static int? GetSubscriptionLimit(
        SubscriptionPlan subscriptionPlan)
    {
        return subscriptionPlan switch
        {
            SubscriptionPlan.Free =>
                FreeSubscriptionLimit,

            SubscriptionPlan.Plus =>
                null,

            SubscriptionPlan.Premium =>
                null,

            _ => throw new ArgumentOutOfRangeException(
                nameof(subscriptionPlan),
                subscriptionPlan,
                "Unsupported subscription plan.")
        };
    }

    public static bool CanUseSavingsPlan(
        SubscriptionPlan subscriptionPlan)
    {
        return subscriptionPlan switch
        {
            SubscriptionPlan.Free =>
                false,

            SubscriptionPlan.Plus =>
                true,

            SubscriptionPlan.Premium =>
                true,

            _ => throw new ArgumentOutOfRangeException(
                nameof(subscriptionPlan),
                subscriptionPlan,
                "Unsupported subscription plan.")
        };
    }

    public static int GetDailySavingsPlanLimit(
        SubscriptionPlan subscriptionPlan)
    {
        return subscriptionPlan switch
        {
            SubscriptionPlan.Plus =>
                PlusDailySavingsPlanLimit,

            SubscriptionPlan.Premium =>
                PremiumDailySavingsPlanLimit,

            SubscriptionPlan.Free =>
                0,

            _ => throw new ArgumentOutOfRangeException(
                nameof(subscriptionPlan),
                subscriptionPlan,
                "Unsupported subscription plan.")
        };
    }
}
