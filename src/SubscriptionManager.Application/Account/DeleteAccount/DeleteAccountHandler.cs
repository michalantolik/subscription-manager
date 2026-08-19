using SubscriptionManager.Application.Authentication;
using SubscriptionManager.Application.Common.Identity;
using SubscriptionManager.Application.Common.Localization;

namespace SubscriptionManager.Application.Account.DeleteAccount;

/// <summary>
/// Handles user account deletion.
/// </summary>
public sealed class DeleteAccountHandler(
    IIdentityService identityService,
    IEmailSender emailSender)
{
    public async Task<DeleteUserResult> HandleAsync(
        DeleteAccountCommand command,
        CancellationToken cancellationToken = default)
    {
        var email =
            await identityService.GetEmailAsync(
                command.UserId,
                cancellationToken);

        var preferences =
            await identityService.GetAccountPreferencesAsync(
                command.UserId,
                cancellationToken);

        var result =
            await identityService.DeleteUserAsync(
                command.UserId,
                cancellationToken);

        if (!result.Succeeded ||
            string.IsNullOrWhiteSpace(email) ||
            preferences is null)
        {
            return result;
        }

        try
        {
            await emailSender.SendAccountDeletedAsync(
                email,
                preferences.Language.ToLanguageCode(),
                cancellationToken);
        }
        catch (Exception)
        {
            // The account has already been deleted.
            // A notification failure must not change the operation result.
        }

        return result;
    }
}
