using SubscriptionManager.Application.Common.Identity;
using SubscriptionManager.Domain.Subscriptions;

namespace SubscriptionManager.Application.Identity.GetBaseCurrency;

public sealed class GetBaseCurrencyHandler(
    IIdentityService identityService)
{
    public async Task<Currency?> HandleAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        return await identityService.GetBaseCurrencyAsync(
            userId,
            cancellationToken);
    }
}
