using SubscriptionManager.Domain.Billing;

namespace SubscriptionManager.Application.Common.Identity;

public static class SubscriptionPlanLimits
{
    public const int FreeDailySavingsPlanLimit = 3;

    public const int PlusDailySavingsPlanLimit = 15;

    public const int PremiumDailySavingsPlanLimit = 50;

    public static int GetDailySavingsPlanLimit(
        SubscriptionPlan subscriptionPlan)
    {
        return subscriptionPlan switch
        {
            SubscriptionPlan.Free =>
                FreeDailySavingsPlanLimit,

            SubscriptionPlan.Plus =>
                PlusDailySavingsPlanLimit,

            SubscriptionPlan.Premium =>
                PremiumDailySavingsPlanLimit,

            _ => throw new ArgumentOutOfRangeException(
                nameof(subscriptionPlan),
                subscriptionPlan,
                "Unsupported subscription plan.")
        };
    }
}
