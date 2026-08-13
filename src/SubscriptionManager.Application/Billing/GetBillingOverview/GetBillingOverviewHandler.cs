using SubscriptionManager.Application.Common.Identity;
using SubscriptionManager.Domain.Billing;

namespace SubscriptionManager.Application.Billing.GetBillingOverview;

/// <summary>
/// Handles billing subscription overview retrieval.
/// </summary>
public sealed class GetBillingOverviewHandler(
    IBillingSubscriptionRepository billingSubscriptionRepository,
    ICurrentUser currentUser)
{
    public async Task<BillingOverviewDto> HandleAsync(
        CancellationToken cancellationToken = default)
    {
        var subscription =
            await billingSubscriptionRepository.GetByUserIdAsync(
                currentUser.UserId,
                cancellationToken);

        if (subscription is null)
        {
            return new BillingOverviewDto(
                SubscriptionPlan.Free,
                null,
                null,
                null,
                null,
                false);
        }

        return new BillingOverviewDto(
            subscription.Plan,
            subscription.BillingInterval,
            subscription.Status,
            subscription.CurrentPeriodStart,
            subscription.CurrentPeriodEnd,
            subscription.CancelAtPeriodEnd);
    }
}
