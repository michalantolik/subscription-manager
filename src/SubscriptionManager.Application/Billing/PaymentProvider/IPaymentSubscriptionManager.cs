using SubscriptionManager.Domain.Billing;

namespace SubscriptionManager.Application.Billing.PaymentProvider;

/// <summary>
/// Manages billing subscriptions through the payment provider.
/// </summary>
public interface IPaymentSubscriptionManager
{
    Task<PaymentSubscriptionChangePreview>
        PreviewChangeAsync(
            string providerSubscriptionId,
            SubscriptionPlan targetPlan,
            BillingInterval targetBillingInterval,
            BillingSubscriptionChangeTiming timing,
            CancellationToken cancellationToken = default);

    Task<PaymentSubscriptionChangeResult> ChangeAsync(
        string providerSubscriptionId,
        SubscriptionPlan targetPlan,
        BillingInterval targetBillingInterval,
        BillingSubscriptionChangeTiming timing,
        CancellationToken cancellationToken = default);

    Task<PaymentSubscriptionState> ScheduleCancellationAsync(
        string providerSubscriptionId,
        CancellationToken cancellationToken = default);

    Task<PaymentSubscriptionState> ResumeAsync(
        string providerSubscriptionId,
        CancellationToken cancellationToken = default);
}
