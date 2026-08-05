using Microsoft.AspNetCore.Components;
using SubscriptionManager.Blazor.Features.Authentication;
using SubscriptionManager.Blazor.Features.Currencies;
using SubscriptionManager.Blazor.Features.Localization;
using System.Globalization;

namespace SubscriptionManager.Blazor.Components.Pages;

public partial class Register
{
    private static readonly IReadOnlyList<(Language Value, string Label)>
        Languages =
        [
            (Language.Polish, "Polski"),
            (Language.English, "English"),
            (Language.German, "Deutsch")
        ];

    [SupplyParameterFromQuery(Name = "errors")]
    private string? Errors { get; set; }

    [SupplyParameterFromQuery(Name = "status")]
    private string? Status { get; set; }

    private Language DefaultLanguage =>
        CultureInfo.CurrentUICulture
            .TwoLetterISOLanguageName switch
        {
            "en" => Language.English,
            "de" => Language.German,
            _ => Language.Polish
        };

    private Currency DefaultBaseCurrency =>
        DefaultLanguage == Language.Polish
            ? Currency.PLN
            : Currency.EUR;

    private bool IsCreated =>
        string.Equals(
            Status,
            "created",
            StringComparison.Ordinal);

    private IReadOnlyList<string> ErrorMessages
    {
        get
        {
            if (string.IsNullOrWhiteSpace(Errors))
            {
                return [];
            }

            var errorCodes = Errors.Split(
                ',',
                StringSplitOptions.RemoveEmptyEntries |
                StringSplitOptions.TrimEntries);

            return AuthenticationErrorCodes.Normalize(errorCodes)
                .Select(GetErrorMessage)
                .ToArray();
        }
    }

    private string GetErrorMessage(string code) => code switch
    {
        AuthenticationErrorCodes.Required =>
            T["Authentication.Register.Required"],

        AuthenticationErrorCodes.PasswordMismatch =>
            T["Authentication.Register.PasswordMismatch"],

        AuthenticationErrorCodes.InvalidEmail or
        AuthenticationErrorCodes.InvalidUserName =>
            T["Authentication.Register.InvalidEmail"],

        AuthenticationErrorCodes.DuplicateEmail or
        AuthenticationErrorCodes.DuplicateUserName =>
            T["Authentication.Register.DuplicateEmail"],

        AuthenticationErrorCodes.PasswordTooShort =>
            T["Authentication.Register.PasswordTooShort"],

        AuthenticationErrorCodes.PasswordRequiresDigit =>
            T["Authentication.Register.PasswordRequiresDigit"],

        AuthenticationErrorCodes.PasswordRequiresLower =>
            T["Authentication.Register.PasswordRequiresLower"],

        AuthenticationErrorCodes.PasswordRequiresUpper =>
            T["Authentication.Register.PasswordRequiresUpper"],

        AuthenticationErrorCodes.PasswordRequiresNonAlphanumeric =>
            T["Authentication.Register.PasswordRequiresSpecialCharacter"],

        AuthenticationErrorCodes.PasswordRequiresUniqueChars =>
            T["Authentication.Register.PasswordRequiresUniqueCharacters"],

        AuthenticationErrorCodes.ServiceUnavailable =>
            T["Authentication.Register.Unavailable"],

        _ =>
            T["Authentication.Register.Error"]
    };
}
