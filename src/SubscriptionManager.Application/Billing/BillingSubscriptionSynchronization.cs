using SubscriptionManager.Application.Billing.PaymentProvider;
using SubscriptionManager.Domain.Billing;

namespace SubscriptionManager.Application.Billing;

/// <summary>
/// Synchronizes billing subscriptions with payment provider state.
/// </summary>
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
