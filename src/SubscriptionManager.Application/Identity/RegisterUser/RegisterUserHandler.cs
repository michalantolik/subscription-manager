using SubscriptionManager.Application.Common.Email;
using SubscriptionManager.Application.Common.Identity;
using SubscriptionManager.Domain.Subscriptions;

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

        var baseCurrency =
            GetDefaultCurrency(
                command.LanguageCode);

        var result =
            await identityService.CreateUserAsync(
                command.Email,
                command.Password,
                baseCurrency,
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
                    command.LanguageCode,
                    cancellationToken);
        }

        return result;
    }

    private static Currency GetDefaultCurrency(
        string? languageCode)
    {
        return languageCode?
            .Trim()
            .ToLowerInvariant() switch
        {
            "de" or "de-de" => Currency.EUR,
            "en" or "en-us" => Currency.EUR,
            _ => Currency.PLN
        };
    }
}
