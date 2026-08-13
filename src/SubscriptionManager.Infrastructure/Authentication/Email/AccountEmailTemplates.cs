namespace SubscriptionManager.Infrastructure.Authentication.Email;

/// <summary>
/// Provides localized account email content.
/// </summary>
internal static class AccountEmailTemplates
{
    public static AccountEmailContent EmailConfirmation(
        string languageCode,
        string confirmationLink)
    {
        return Normalize(languageCode) switch
        {
            "en" => new AccountEmailContent(
                "Confirm your Subscription Manager account",
                $"Confirm your email address by opening this link:\n\n{confirmationLink}\n\nIf you did not create this account, you can ignore this message."),

            "de" => new AccountEmailContent(
                "Subscription Manager-Konto bestätigen",
                $"Bestätigen Sie Ihre E-Mail-Adresse über diesen Link:\n\n{confirmationLink}\n\nFalls Sie dieses Konto nicht erstellt haben, können Sie diese Nachricht ignorieren."),

            _ => new AccountEmailContent(
                "Potwierdź konto Subscription Manager",
                $"Potwierdź swój adres e-mail, otwierając ten link:\n\n{confirmationLink}\n\nJeżeli nie zakładałeś tego konta, zignoruj tę wiadomość.")
        };
    }

    public static AccountEmailContent PasswordReset(
        string languageCode,
        string resetLink)
    {
        return Normalize(languageCode) switch
        {
            "en" => new AccountEmailContent(
                "Reset your Subscription Manager password",
                $"Set a new password by opening this link:\n\n{resetLink}\n\nIf you did not request a password reset, you can ignore this message."),

            "de" => new AccountEmailContent(
                "Subscription Manager-Passwort zurücksetzen",
                $"Legen Sie über diesen Link ein neues Passwort fest:\n\n{resetLink}\n\nFalls Sie keine Passwortänderung angefordert haben, können Sie diese Nachricht ignorieren."),

            _ => new AccountEmailContent(
                "Zresetuj hasło do Subscription Manager",
                $"Ustaw nowe hasło, otwierając ten link:\n\n{resetLink}\n\nJeżeli nie prosiłeś o zmianę hasła, zignoruj tę wiadomość.")
        };
    }

    private static string Normalize(string? languageCode)
    {
        if (string.IsNullOrWhiteSpace(languageCode))
        {
            return "pl";
        }

        return languageCode.Trim().ToLowerInvariant() switch
        {
            "en" or "en-us" => "en",
            "de" or "de-de" => "de",
            _ => "pl"
        };
    }
}

/// <summary>
/// Contains the subject and text body of an account email.
/// </summary>
internal sealed record AccountEmailContent(
    string Subject,
    string TextBody);
