using SubscriptionManager.Application.Common.Identity;

namespace SubscriptionManager.Application.Account.DeleteAccount;

/// <summary>
/// Handles user account deletion.
/// </summary>
public sealed class DeleteAccountHandler(
    IIdentityService identityService)
{
    public async Task<DeleteUserResult> HandleAsync(
        DeleteAccountCommand command,
        CancellationToken cancellationToken = default)
    {
        return await identityService.DeleteUserAsync(
            command.UserId,
            cancellationToken);
    }
}
