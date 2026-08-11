using SubscriptionManager.Domain.Billing;

namespace SubscriptionManager.Application.Billing;

internal static class BillingSubscriptionSynchronization
{
    public static void Apply(
        BillingSubscription subscription,
        PaymentSubscriptionState providerState)
    {
        subscription.Synchronize(
            providerState.Plan,
            providerState.BillingInterval,
            providerState.Status,
            providerState.ProviderPriceId,
            providerState.CurrentPeriodStart,
            providerState.CurrentPeriodEnd,
            providerState.CancelAtPeriodEnd);
    }
}
