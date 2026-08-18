using System.Net;

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
            "en" => Create(
                language: "en",
                subject: "Confirm your Subscription Manager account",
                eyebrow: "ACCOUNT CONFIRMATION",
                title: "Confirm your email address",
                description: "Finish creating your Subscription Manager account by confirming your email address.",
                actionText: "Confirm email address",
                actionLink: confirmationLink,
                fallbackText: "If the button does not work, open this link:",
                securityText: "If you did not create this account, you can ignore this message."),

            "de" => Create(
                language: "de",
                subject: "Subscription Manager-Konto bestätigen",
                eyebrow: "KONTOBESTÄTIGUNG",
                title: "E-Mail-Adresse bestätigen",
                description: "Schließen Sie die Erstellung Ihres Subscription Manager-Kontos ab, indem Sie Ihre E-Mail-Adresse bestätigen.",
                actionText: "E-Mail-Adresse bestätigen",
                actionLink: confirmationLink,
                fallbackText: "Falls die Schaltfläche nicht funktioniert, öffnen Sie diesen Link:",
                securityText: "Falls Sie dieses Konto nicht erstellt haben, können Sie diese Nachricht ignorieren."),

            _ => Create(
                language: "pl",
                subject: "Potwierdź konto Subscription Manager",
                eyebrow: "POTWIERDZENIE KONTA",
                title: "Potwierdź swój adres e-mail",
                description: "Dokończ tworzenie konta Subscription Manager, potwierdzając swój adres e-mail.",
                actionText: "Potwierdź adres e-mail",
                actionLink: confirmationLink,
                fallbackText: "Jeżeli przycisk nie działa, otwórz ten link:",
                securityText: "Jeżeli nie zakładałeś tego konta, możesz zignorować tę wiadomość.")
        };
    }

    public static AccountEmailContent PasswordReset(
        string languageCode,
        string resetLink)
    {
        return Normalize(languageCode) switch
        {
            "en" => Create(
                language: "en",
                subject: "Reset your Subscription Manager password",
                eyebrow: "PASSWORD RESET",
                title: "Reset your password",
                description: "Use the button below to set a new password for your Subscription Manager account.",
                actionText: "Reset password",
                actionLink: resetLink,
                fallbackText: "If the button does not work, open this link:",
                securityText: "If you did not request a password reset, you can ignore this message."),

            "de" => Create(
                language: "de",
                subject: "Subscription Manager-Passwort zurücksetzen",
                eyebrow: "PASSWORT ZURÜCKSETZEN",
                title: "Passwort zurücksetzen",
                description: "Verwenden Sie die Schaltfläche unten, um ein neues Passwort für Ihr Subscription Manager-Konto festzulegen.",
                actionText: "Passwort zurücksetzen",
                actionLink: resetLink,
                fallbackText: "Falls die Schaltfläche nicht funktioniert, öffnen Sie diesen Link:",
                securityText: "Falls Sie keine Passwortänderung angefordert haben, können Sie diese Nachricht ignorieren."),

            _ => Create(
                language: "pl",
                subject: "Zresetuj hasło do Subscription Manager",
                eyebrow: "RESET HASŁA",
                title: "Zresetuj swoje hasło",
                description: "Użyj poniższego przycisku, aby ustawić nowe hasło do konta Subscription Manager.",
                actionText: "Zresetuj hasło",
                actionLink: resetLink,
                fallbackText: "Jeżeli przycisk nie działa, otwórz ten link:",
                securityText: "Jeżeli nie prosiłeś o reset hasła, możesz zignorować tę wiadomość.")
        };
    }

    private static AccountEmailContent Create(
        string language,
        string subject,
        string eyebrow,
        string title,
        string description,
        string actionText,
        string actionLink,
        string fallbackText,
        string securityText)
    {
        var textBody =
            $"{title}\n\n" +
            $"{description}\n\n" +
            $"{actionLink}\n\n" +
            $"{securityText}";

        var htmlBody = BuildHtml(
            language,
            eyebrow,
            title,
            description,
            actionText,
            actionLink,
            fallbackText,
            securityText);

        return new AccountEmailContent(
            subject,
            textBody,
            htmlBody);
    }

    private static string BuildHtml(
        string language,
        string eyebrow,
        string title,
        string description,
        string actionText,
        string actionLink,
        string fallbackText,
        string securityText)
    {
        var brandIconUrl = BuildBrandIconUrl(actionLink);

        var encodedLanguage = WebUtility.HtmlEncode(language);
        var encodedEyebrow = WebUtility.HtmlEncode(eyebrow);
        var encodedTitle = WebUtility.HtmlEncode(title);
        var encodedDescription = WebUtility.HtmlEncode(description);
        var encodedActionText = WebUtility.HtmlEncode(actionText);
        var encodedActionLink = WebUtility.HtmlEncode(actionLink);
        var encodedBrandIconUrl = WebUtility.HtmlEncode(brandIconUrl);
        var encodedFallbackText = WebUtility.HtmlEncode(fallbackText);
        var encodedSecurityText = WebUtility.HtmlEncode(securityText);

        return $$"""
            <!doctype html>
            <html lang="{{encodedLanguage}}">
            <head>
                <meta charset="utf-8">
                <meta name="viewport" content="width=device-width, initial-scale=1">
                <title>{{encodedTitle}}</title>
            </head>
            <body style="margin:0;padding:0;background:#f5f7f6;font-family:Arial,Helvetica,sans-serif;color:#17211b;">
                <table role="presentation" width="100%" cellspacing="0" cellpadding="0" border="0" style="width:100%;background:#f5f7f6;">
                    <tr>
                        <td align="center" style="padding:40px 16px;">
                            <table role="presentation" width="100%" cellspacing="0" cellpadding="0" border="0" style="width:100%;max-width:560px;">
                                <tr>
                                    <td style="padding:0 0 16px 0;">
                                        <table role="presentation" cellspacing="0" cellpadding="0" border="0">
                                            <tr>
                                                <td width="40" height="40" style="width:40px;height:40px;line-height:0;">
                                                    <img src="{{encodedBrandIconUrl}}"
                                                         width="40"
                                                         height="40"
                                                         alt=""
                                                         style="display:block;width:40px;height:40px;border:0;outline:none;text-decoration:none;">
                                                </td>
                                                <td style="padding-left:10px;font-size:15px;font-weight:700;line-height:18px;color:#17211b;">
                                                    Subscription<br>
                                                    <span style="color:#42a65f;">Manager</span>
                                                </td>
                                            </tr>
                                        </table>
                                    </td>
                                </tr>

                                <tr>
                                    <td style="background:#ffffff;border:1px solid #e2e8e4;border-radius:16px;padding:32px;">
                                        <div style="margin:0 0 10px 0;font-size:12px;font-weight:700;letter-spacing:0.08em;color:#42a65f;">
                                            {{encodedEyebrow}}
                                        </div>

                                        <h1 style="margin:0 0 12px 0;font-size:26px;line-height:34px;font-weight:700;color:#17211b;">
                                            {{encodedTitle}}
                                        </h1>

                                        <p style="margin:0 0 24px 0;font-size:15px;line-height:24px;color:#66736b;">
                                            {{encodedDescription}}
                                        </p>

                                        <table role="presentation" cellspacing="0" cellpadding="0" border="0" style="margin:0 0 28px 0;">
                                            <tr>
                                                <td style="border-radius:10px;background:#42a65f;">
                                                    <a href="{{encodedActionLink}}"
                                                       style="display:inline-block;padding:13px 20px;font-size:15px;font-weight:700;line-height:20px;color:#ffffff;text-decoration:none;">
                                                        {{encodedActionText}}
                                                    </a>
                                                </td>
                                            </tr>
                                        </table>

                                        <p style="margin:0 0 8px 0;font-size:13px;line-height:20px;color:#66736b;">
                                            {{encodedFallbackText}}
                                        </p>

                                        <p style="margin:0 0 24px 0;font-size:12px;line-height:18px;word-break:break-all;">
                                            <a href="{{encodedActionLink}}"
                                               style="color:#3975d6;text-decoration:none;">
                                                {{encodedActionLink}}
                                            </a>
                                        </p>

                                        <div style="border-top:1px solid #e8ece9;padding-top:20px;">
                                            <p style="margin:0;font-size:13px;line-height:20px;color:#7a867f;">
                                                {{encodedSecurityText}}
                                            </p>
                                        </div>
                                    </td>
                                </tr>

                                <tr>
                                    <td style="padding:18px 4px 0 4px;text-align:center;font-size:12px;line-height:18px;color:#8a958e;">
                                        Subscription Manager · submanager.dev
                                    </td>
                                </tr>
                            </table>
                        </td>
                    </tr>
                </table>
            </body>
            </html>
            """;
    }

    private static string BuildBrandIconUrl(
        string actionLink)
    {
        var actionUri =
            new Uri(
                actionLink,
                UriKind.Absolute);

        return new Uri(
            actionUri,
            "/images/branding/app-icon-brand.png")
            .AbsoluteUri;
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
/// Contains the subject, text body and HTML body of an account email.
/// </summary>
internal sealed record AccountEmailContent(
    string Subject,
    string TextBody,
    string HtmlBody);
