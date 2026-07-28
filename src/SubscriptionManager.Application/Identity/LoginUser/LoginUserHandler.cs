using SubscriptionManager.Application.Common.Authentication;
using SubscriptionManager.Application.Common.Identity;

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

        return LoginUserResult.Success(accessToken);
    }
}

public sealed record LoginUserResult(
    bool Succeeded,
    string? AccessToken,
    IReadOnlyCollection<IdentityServiceError> Errors)
{
    public static LoginUserResult Success(
        string accessToken)
        => new(true, accessToken, []);

    public static LoginUserResult Failure(
        IEnumerable<IdentityServiceError> errors)
        => new(false, null, errors.ToArray());
}
