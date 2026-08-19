using SubscriptionManager.Application.Common.Identity;
using SubscriptionManager.Application.Common.Localization;

namespace SubscriptionManager.Application.Authentication.ResetPassword;

/// <summary>
/// Handles user password reset.
/// </summary>
public sealed class ResetPasswordHandler(
    IIdentityService identityService,
    IEmailSender emailSender)
{
    public async Task<ResetPasswordResult> HandleAsync(
        ResetPasswordCommand command,
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
            await identityService.ResetPasswordAsync(
                command.UserId,
                command.ResetToken,
                command.NewPassword,
                cancellationToken);

        if (!result.Succeeded ||
            string.IsNullOrWhiteSpace(email) ||
            preferences is null)
        {
            return result;
        }

        try
        {
            await emailSender.SendPasswordChangedAsync(
                email,
                preferences.Language.ToLanguageCode(),
                cancellationToken);
        }
        catch (Exception)
        {
            // The password has already been changed.
            // A notification failure must not change the operation result.
        }

        return result;
    }
}
