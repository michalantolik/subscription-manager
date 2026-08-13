namespace SubscriptionManager.Web.Features.Authentication;

public static class AuthenticationErrorCodes
{
    public const string Required = "Required";
    public const string PasswordMismatch = "PasswordMismatch";
    public const string ServiceUnavailable = "ServiceUnavailable";
    public const string UnexpectedError = "UnexpectedError";
    public const string SessionExpired = "SessionExpired";

    public const string InvalidEmail = "InvalidEmail";
    public const string InvalidUserName = "InvalidUserName";
    public const string DuplicateEmail = "DuplicateEmail";
    public const string DuplicateUserName = "DuplicateUserName";
    public const string PasswordTooShort = "PasswordTooShort";
    public const string PasswordRequiresDigit = "PasswordRequiresDigit";
    public const string PasswordRequiresLower = "PasswordRequiresLower";
    public const string PasswordRequiresUpper = "PasswordRequiresUpper";
    public const string PasswordRequiresNonAlphanumeric =
        "PasswordRequiresNonAlphanumeric";
    public const string PasswordRequiresUniqueChars =
        "PasswordRequiresUniqueChars";

    public const string InvalidCredentials = "InvalidCredentials";
    public const string EmailNotConfirmed = "EmailNotConfirmed";

    private static readonly HashSet<string> KnownCodes =
    [
        Required,
        PasswordMismatch,
        ServiceUnavailable,
        UnexpectedError,
        SessionExpired,
        InvalidEmail,
        InvalidUserName,
        DuplicateEmail,
        DuplicateUserName,
        PasswordTooShort,
        PasswordRequiresDigit,
        PasswordRequiresLower,
        PasswordRequiresUpper,
        PasswordRequiresNonAlphanumeric,
        PasswordRequiresUniqueChars,
        InvalidCredentials,
        EmailNotConfirmed
    ];

    public static IReadOnlyCollection<string> Normalize(
        IEnumerable<string> codes)
    {
        var normalizedCodes = codes
            .Where(code => !string.IsNullOrWhiteSpace(code))
            .Select(code => code.Trim())
            .Where(KnownCodes.Contains)
            .Distinct(StringComparer.Ordinal)
            .ToArray();

        return normalizedCodes.Length > 0
            ? normalizedCodes
            : [UnexpectedError];
    }
}
