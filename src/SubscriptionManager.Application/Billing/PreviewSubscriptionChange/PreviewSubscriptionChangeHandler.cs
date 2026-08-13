using SubscriptionManager.Application.Billing.PaymentProvider;
using SubscriptionManager.Application.Common.Identity;
using SubscriptionManager.Domain.Billing;

namespace SubscriptionManager.Application.Billing.PreviewSubscriptionChange;

/// <summary>
/// Handles billing subscription change preview.
/// </summary>
public sealed class PreviewSubscriptionChangeHandler(
    ICurrentUser currentUser,
    IBillingSubscriptionRepository billingSubscriptionRepository,
    IPaymentSubscriptionManager paymentSubscriptionManager,
    TimeProvider timeProvider)
{
    public async Task<SubscriptionChangePreviewDto> HandleAsync(
        PreviewSubscriptionChangeCommand command,
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

        var preview =
            await paymentSubscriptionManager
                .PreviewChangeAsync(
                    subscription.ProviderSubscriptionId,
                    command.TargetPlan,
                    command.TargetBillingInterval,
                    timing,
                    cancellationToken);

        return new SubscriptionChangePreviewDto(
            subscription.Plan,
            subscription.BillingInterval,
            command.TargetPlan,
            command.TargetBillingInterval,
            timing,
            preview.AmountDueNow,
            preview.Currency,
            preview.EffectiveAt);
    }
}
