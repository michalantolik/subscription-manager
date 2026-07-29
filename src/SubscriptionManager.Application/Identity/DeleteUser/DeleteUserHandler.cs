using SubscriptionManager.Application.Common.Identity;

namespace SubscriptionManager.Application.Identity.DeleteUser;

public sealed class DeleteUserHandler(
    IIdentityService identityService)
{
    public async Task<DeleteUserResult> HandleAsync(
        DeleteUserCommand command,
        CancellationToken cancellationToken = default)
    {
        return await identityService.DeleteUserAsync(
            command.UserId,
            cancellationToken);
    }
}
