using SubscriptionManager.Application.Common.Localization;
using SubscriptionManager.Domain.Billing;
using SubscriptionManager.Domain.Subscriptions;

namespace SubscriptionManager.Application.Common.Identity;

public interface IIdentityService
{
    Task<CreateUserResult> CreateUserAsync(
        string email,
        string password,
        Language language,
        Currency baseCurrency,
        CancellationToken cancellationToken = default);

    Task<AccountPreferences?> GetAccountPreferencesAsync(
        Guid userId,
        CancellationToken cancellationToken = default);

    Task<bool> UpdateAccountPreferencesAsync(
        Guid userId,
        Language language,
        Currency baseCurrency,
        CancellationToken cancellationToken = default);

    Task<Currency?> GetBaseCurrencyAsync(
        Guid userId,
        CancellationToken cancellationToken = default);

    Task<string?> GetEmailAsync(
        Guid userId,
        CancellationToken cancellationToken = default);

    Task<SubscriptionPlan?> GetSubscriptionPlanAsync(
        Guid userId,
        CancellationToken cancellationToken = default);

    Task<string?> GenerateEmailConfirmationTokenAsync(
        Guid userId,
        CancellationToken cancellationToken = default);

    Task<ConfirmEmailResult> ConfirmEmailAsync(
        Guid userId,
        string confirmationToken,
        CancellationToken cancellationToken = default);

    Task<AuthenticateUserResult> AuthenticateUserAsync(
        string email,
        string password,
        CancellationToken cancellationToken = default);

    Task<PasswordResetToken?> GeneratePasswordResetTokenAsync(
        string email,
        CancellationToken cancellationToken = default);

    Task<ResetPasswordResult> ResetPasswordAsync(
        Guid userId,
        string resetToken,
        string newPassword,
        CancellationToken cancellationToken = default);

    Task<DeleteUserResult> DeleteUserAsync(
        Guid userId,
        CancellationToken cancellationToken = default);
}

public sealed record IdentityServiceError(
    string Code,
    string Description);

public sealed record CreateUserResult(
    bool Succeeded,
    Guid? UserId,
    IReadOnlyCollection<IdentityServiceError> Errors)
{
    public static CreateUserResult Success(
        Guid userId)
        => new(
            true,
            userId,
            []);

    public static CreateUserResult Failure(
        IEnumerable<IdentityServiceError> errors)
        => new(
            false,
            null,
            errors.ToArray());
}

public sealed record ConfirmEmailResult(
    bool Succeeded,
    IReadOnlyCollection<IdentityServiceError> Errors)
{
    public static ConfirmEmailResult Success()
        => new(
            true,
            []);

    public static ConfirmEmailResult Failure(
        IEnumerable<IdentityServiceError> errors)
        => new(
            false,
            errors.ToArray());
}

public sealed record AuthenticateUserResult(
    bool Succeeded,
    Guid? UserId,
    Language? Language,
    SubscriptionPlan? SubscriptionPlan,
    IReadOnlyCollection<IdentityServiceError> Errors)
{
    public static AuthenticateUserResult Success(
        Guid userId,
        Language language,
        SubscriptionPlan subscriptionPlan)
        => new(
            true,
            userId,
            language,
            subscriptionPlan,
            []);

    public static AuthenticateUserResult Failure(
        IEnumerable<IdentityServiceError> errors)
        => new(
            false,
            null,
            null,
            null,
            errors.ToArray());
}

public sealed record PasswordResetToken(
    Guid UserId,
    string Email,
    string Token);

public sealed record ResetPasswordResult(
    bool Succeeded,
    IReadOnlyCollection<IdentityServiceError> Errors)
{
    public static ResetPasswordResult Success()
        => new(
            true,
            []);

    public static ResetPasswordResult Failure(
        IEnumerable<IdentityServiceError> errors)
        => new(
            false,
            errors.ToArray());
}

public sealed record DeleteUserResult(
    bool Succeeded,
    IReadOnlyCollection<IdentityServiceError> Errors)
{
    public static DeleteUserResult Success()
        => new(
            true,
            []);

    public static DeleteUserResult Failure(
        IEnumerable<IdentityServiceError> errors)
        => new(
            false,
            errors.ToArray());
}
