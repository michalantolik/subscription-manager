using SubscriptionManager.Application.Common.Email;
using SubscriptionManager.Application.Common.Identity;

namespace SubscriptionManager.Application.Identity.ForgotPassword;

public sealed class ForgotPasswordHandler(
    IIdentityService identityService,
    IEmailSender emailSender)
{
    public async Task HandleAsync(
        ForgotPasswordCommand command,
        CancellationToken cancellationToken = default)
    {
        var passwordResetToken =
            await identityService.GeneratePasswordResetTokenAsync(
                command.Email,
                cancellationToken);

        if (passwordResetToken is null)
        {
            return;
        }

        await emailSender.SendPasswordResetAsync(
            passwordResetToken.Email,
            passwordResetToken.UserId,
            passwordResetToken.Token,
            command.LanguageCode,
            cancellationToken);
    }
}
