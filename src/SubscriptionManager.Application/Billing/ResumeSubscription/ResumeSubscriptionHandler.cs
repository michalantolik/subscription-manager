using SubscriptionManager.Application.Common.Identity;
using SubscriptionManager.Domain.Billing;

namespace SubscriptionManager.Application.Billing.ResumeSubscription;

public sealed class ResumeSubscriptionHandler(
    ICurrentUser currentUser,
    IBillingSubscriptionRepository billingSubscriptionRepository,
    IPaymentSubscriptionManager paymentSubscriptionManager)
{
    public async Task HandleAsync(
        ResumeSubscriptionCommand command,
        CancellationToken cancellationToken = default)
    {
        var subscription =
            await billingSubscriptionRepository
                .GetByUserIdAsync(
                    currentUser.UserId,
                    cancellationToken);

        if (subscription is null)
        {
            throw new BillingSubscriptionResumeUnavailableException(
                "A paid billing subscription is required.");
        }

        if (string.IsNullOrWhiteSpace(
                subscription.ProviderSubscriptionId))
        {
            throw new BillingSubscriptionResumeUnavailableException(
                "The billing subscription is not linked to the payment provider.");
        }

        if (subscription.Status is
            BillingSubscriptionStatus.Canceled or
            BillingSubscriptionStatus.IncompleteExpired)
        {
            throw new BillingSubscriptionResumeUnavailableException(
                "The billing subscription has already ended.");
        }

        if (!subscription.CancelAtPeriodEnd)
        {
            return;
        }

        var providerState =
            await paymentSubscriptionManager.ResumeAsync(
                subscription.ProviderSubscriptionId,
                cancellationToken);

        BillingSubscriptionSynchronization.Apply(
            subscription,
            providerState);

        await billingSubscriptionRepository.SaveChangesAsync(
            cancellationToken);
    }
}
