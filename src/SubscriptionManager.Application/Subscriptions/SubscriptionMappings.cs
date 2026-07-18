using SubscriptionManager.Domain.Subscriptions;

namespace SubscriptionManager.Application.Subscriptions;

internal static class SubscriptionMappings
{
    public static SubscriptionDto ToDto(this Subscription subscription)
    {
        return new SubscriptionDto(
            subscription.Id,
            subscription.Name,
            subscription.Amount,
            subscription.Currency,
            subscription.BillingPeriod,
            subscription.StartDate,
            subscription.EndDate,
            subscription.IsActive,
            subscription.MonthlyEquivalentAmount,
            subscription.YearlyEquivalentAmount);
    }
}
