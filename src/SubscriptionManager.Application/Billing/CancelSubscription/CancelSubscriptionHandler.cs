using SubscriptionManager.Application.Billing.PaymentProvider;
using SubscriptionManager.Application.Common.Identity;
using SubscriptionManager.Domain.Billing;

namespace SubscriptionManager.Application.Billing.CancelSubscription;

/// <summary>
/// Handles billing subscription cancellation.
/// </summary>
public sealed class CancelSubscriptionHandler(
    ICurrentUser currentUser,
    IBillingSubscriptionRepository billingSubscriptionRepository,
    IPaymentSubscriptionManager paymentSubscriptionManager)
{
    public async Task HandleAsync(
        CancelSubscriptionCommand command,
        CancellationToken cancellationToken = default)
    {
        var subscription =
            await billingSubscriptionRepository
                .GetByUserIdAsync(
                    currentUser.UserId,
                    cancellationToken);

        if (subscription is null)
        {
            throw new BillingSubscriptionCancellationUnavailableException(
                "A paid billing subscription is required.");
        }

        if (string.IsNullOrWhiteSpace(
                subscription.ProviderSubscriptionId))
        {
            throw new BillingSubscriptionCancellationUnavailableException(
                "The billing subscription is not linked to the payment provider.");
        }

        if (subscription.Status is
            BillingSubscriptionStatus.Canceled or
            BillingSubscriptionStatus.IncompleteExpired)
        {
            throw new BillingSubscriptionCancellationUnavailableException(
                "The billing subscription has already ended.");
        }

        if (subscription.CancelAtPeriodEnd)
        {
            return;
        }

        var providerState =
            await paymentSubscriptionManager
                .ScheduleCancellationAsync(
                    subscription.ProviderSubscriptionId,
                    cancellationToken);

        BillingSubscriptionSynchronization.Apply(
            subscription,
            providerState);

        await billingSubscriptionRepository.SaveChangesAsync(
            cancellationToken);
    }
}
