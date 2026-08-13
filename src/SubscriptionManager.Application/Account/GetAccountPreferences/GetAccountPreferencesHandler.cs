using SubscriptionManager.Application.Common.Identity;

namespace SubscriptionManager.Application.Account.GetAccountPreferences;

/// <summary>
/// Handles user account preferences retrieval.
/// </summary>
public sealed class GetAccountPreferencesHandler(
    IIdentityService identityService)
{
    public Task<AccountPreferences?> HandleAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        return identityService.GetAccountPreferencesAsync(
            userId,
            cancellationToken);
    }
}
