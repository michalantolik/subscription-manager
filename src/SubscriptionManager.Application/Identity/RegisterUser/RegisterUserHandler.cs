using SubscriptionManager.Application.Common.Email;
using SubscriptionManager.Application.Common.Identity;
using SubscriptionManager.Application.Common.Localization;

namespace SubscriptionManager.Application.Identity.RegisterUser;

public sealed class RegisterUserHandler(
    IIdentityService identityService,
    IEmailSender emailSender)
{
    public async Task<CreateUserResult> HandleAsync(
        RegisterUserCommand command,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);

        var result =
            await identityService.CreateUserAsync(
                command.Email,
                command.Password,
                command.Language,
                command.BaseCurrency,
                cancellationToken);

        if (!result.Succeeded)
        {
            return result;
        }

        var userId = result.UserId!.Value;

        var confirmationToken =
            await identityService
                .GenerateEmailConfirmationTokenAsync(
                    userId,
                    cancellationToken);

        if (confirmationToken is not null)
        {
            await emailSender
                .SendEmailConfirmationAsync(
                    command.Email,
                    userId,
                    confirmationToken,
                    command.Language.ToLanguageCode(),
                    cancellationToken);
        }

        return result;
    }
}
