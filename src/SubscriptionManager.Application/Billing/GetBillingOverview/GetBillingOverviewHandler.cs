using SubscriptionManager.Application.Common.Identity;
using SubscriptionManager.Domain.Billing;

namespace SubscriptionManager.Application.Billing.GetBillingOverview;

public sealed class GetBillingOverviewHandler
{
    private readonly IIdentityService _identityService;
    private readonly ICurrentUser _currentUser;

    public GetBillingOverviewHandler(
        IIdentityService identityService,
        ICurrentUser currentUser)
    {
        _identityService = identityService;
        _currentUser = currentUser;
    }

    public async Task<BillingOverviewDto> HandleAsync(
        CancellationToken cancellationToken = default)
    {
        var userId =
            _currentUser.UserId;

        var subscriptionPlan =
            await _identityService.GetSubscriptionPlanAsync(
                userId,
                cancellationToken);

        if (subscriptionPlan is null)
        {
            throw new InvalidOperationException(
                "The current user's subscription plan is unavailable.");
        }

        if (subscriptionPlan == SubscriptionPlan.Free)
        {
            return new BillingOverviewDto(
                SubscriptionPlan.Free,
                null,
                null,
                null,
                null,
                false);
        }

        throw new NotImplementedException();
    }
}
