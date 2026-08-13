using SubscriptionManager.Web.Common.Localization;

namespace SubscriptionManager.Web.Features.Authentication;

/// <summary>
/// Represents an error returned by an authentication operation.
/// </summary>
public sealed record AuthenticationError(
    string Code,
    string Description);

/// <summary>
/// Represents the result of an authentication operation.
/// </summary>
public sealed record AuthenticationOperationResult(
    bool Succeeded,
    IReadOnlyCollection<AuthenticationError> Errors)
{
    public static AuthenticationOperationResult Success()
        => new(true, []);

    public static AuthenticationOperationResult Failure(
        IEnumerable<AuthenticationError> errors)
        => new(false, errors.ToArray());
}

/// <summary>
/// Represents the result of a login operation.
/// </summary>
public sealed record LoginOperationResult(
    bool Succeeded,
    string? AccessToken,
    Language? Language,
    string? SubscriptionPlan,
    IReadOnlyCollection<AuthenticationError> Errors)
{
    public static LoginOperationResult Success(
        string accessToken,
        Language language,
        string subscriptionPlan)
        => new(
            true,
            accessToken,
            language,
            subscriptionPlan,
            []);

    public static LoginOperationResult Failure(
        IEnumerable<AuthenticationError> errors)
        => new(
            false,
            null,
            null,
            null,
            errors.ToArray());
}
