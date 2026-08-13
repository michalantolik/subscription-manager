using SubscriptionManager.Application.Common.Identity;

namespace SubscriptionManager.Application.Account.UpdateAccountPreferences;

/// <summary>
/// Handles user account preferences update.
/// </summary>
public sealed class UpdateAccountPreferencesHandler(
    IIdentityService identityService)
{
    public Task<bool> HandleAsync(
        UpdateAccountPreferencesCommand command,
        CancellationToken cancellationToken = default)
    {
        return identityService.UpdateAccountPreferencesAsync(
            command.UserId,
            command.Language,
            command.BaseCurrency,
            cancellationToken);
    }
}
