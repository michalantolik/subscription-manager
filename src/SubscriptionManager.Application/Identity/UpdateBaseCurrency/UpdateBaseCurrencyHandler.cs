using SubscriptionManager.Application.Common.Identity;

namespace SubscriptionManager.Application.Identity.UpdateBaseCurrency;

public sealed class UpdateBaseCurrencyHandler(
    IIdentityService identityService)
{
    public async Task<bool> HandleAsync(
        UpdateBaseCurrencyCommand command,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);

        return await identityService.UpdateBaseCurrencyAsync(
            command.UserId,
            command.BaseCurrency,
            cancellationToken);
    }
}
