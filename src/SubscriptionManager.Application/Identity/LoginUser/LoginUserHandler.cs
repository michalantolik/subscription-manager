using SubscriptionManager.Application.Common.Authentication;
using SubscriptionManager.Application.Common.Identity;
using SubscriptionManager.Application.Common.Localization;

namespace SubscriptionManager.Application.Identity.LoginUser;

public sealed class LoginUserHandler(
    IIdentityService identityService,
    IAccessTokenGenerator accessTokenGenerator)
{
    public async Task<LoginUserResult> HandleAsync(
        LoginUserCommand command,
        CancellationToken cancellationToken = default)
    {
        var authenticationResult =
            await identityService.AuthenticateUserAsync(
                command.Email,
                command.Password,
                cancellationToken);

        if (!authenticationResult.Succeeded)
        {
            return LoginUserResult.Failure(
                authenticationResult.Errors);
        }

        var accessToken = accessTokenGenerator.GenerateToken(
            authenticationResult.UserId!.Value);

        return LoginUserResult.Success(
            accessToken,
            authenticationResult.Language!.Value,
            authenticationResult.SubscriptionPlan!.Value);
    }
}

public sealed record LoginUserResult(
    bool Succeeded,
    string? AccessToken,
    Language? Language,
    SubscriptionPlan? SubscriptionPlan,
    IReadOnlyCollection<IdentityServiceError> Errors)
{
    public static LoginUserResult Success(
        string accessToken,
        Language language,
        SubscriptionPlan subscriptionPlan)
        => new(
            true,
            accessToken,
            language,
            subscriptionPlan,
            []);

    public static LoginUserResult Failure(
        IEnumerable<IdentityServiceError> errors)
        => new(
            false,
            null,
            null,
            null,
            errors.ToArray());
}
