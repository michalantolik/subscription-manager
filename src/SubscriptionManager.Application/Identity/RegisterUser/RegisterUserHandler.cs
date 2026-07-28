using SubscriptionManager.Application.Common.Identity;

namespace SubscriptionManager.Application.Identity.RegisterUser;

public sealed class RegisterUserHandler(
    IIdentityService identityService)
{
    public Task<CreateUserResult> HandleAsync(
        RegisterUserCommand command,
        CancellationToken cancellationToken = default)
    {
        return identityService.CreateUserAsync(
            command.Email,
            command.Password,
            cancellationToken);
    }
}
