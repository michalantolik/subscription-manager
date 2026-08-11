using SubscriptionManager.Application.Billing.PreviewSubscriptionChange;
using SubscriptionManager.Application.Common.Identity;
using SubscriptionManager.Domain.Billing;

namespace SubscriptionManager.Application.Billing.ChangeSubscription;

public sealed class ChangeSubscriptionHandler(
    ICurrentUser currentUser,
    IBillingSubscriptionRepository billingSubscriptionRepository,
    IPaymentSubscriptionManager paymentSubscriptionManager,
    TimeProvider timeProvider)
{
    public async Task HandleAsync(
        ChangeSubscriptionCommand command,
        CancellationToken cancellationToken = default)
    {
        var subscription =
            await billingSubscriptionRepository
                .GetByUserIdAsync(
                    currentUser.UserId,
                    cancellationToken);

        if (subscription is null)
        {
            throw new BillingSubscriptionChangeUnavailableException(
                "A paid billing subscription is required.");
        }

        if (string.IsNullOrWhiteSpace(
                subscription.ProviderSubscriptionId))
        {
            throw new BillingSubscriptionChangeUnavailableException(
                "The billing subscription is not linked to the payment provider.");
        }

        if (!subscription.GrantsPaidAccessAt(
                timeProvider.GetUtcNow()))
        {
            throw new BillingSubscriptionChangeUnavailableException(
                "The billing subscription is not active.");
        }

        if (subscription.CancelAtPeriodEnd)
        {
            throw new BillingSubscriptionChangeUnavailableException(
                "The scheduled cancellation must be resumed before changing the subscription.");
        }

        var timing =
            BillingSubscriptionChangePolicy.GetTiming(
                subscription.Plan,
                subscription.BillingInterval,
                command.TargetPlan,
                command.TargetBillingInterval);

        var result =
            await paymentSubscriptionManager.ChangeAsync(
                subscription.ProviderSubscriptionId,
                command.TargetPlan,
                command.TargetBillingInterval,
                timing,
                cancellationToken);

        if (result.UpdatedSubscription is null)
        {
            return;
        }

        BillingSubscriptionSynchronization.Apply(
            subscription,
            result.UpdatedSubscription);

        await billingSubscriptionRepository.SaveChangesAsync(
            cancellationToken);
    }
}
