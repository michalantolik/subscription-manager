using SubscriptionManager.Application.Common.Identity;

namespace SubscriptionManager.Application.Identity.ConfirmEmail;

public sealed class ConfirmEmailHandler(
    IIdentityService identityService)
{
    public Task<ConfirmEmailResult> HandleAsync(
        ConfirmEmailCommand command,
        CancellationToken cancellationToken = default)
    {
        return identityService.ConfirmEmailAsync(
            command.UserId,
            command.ConfirmationToken,
            cancellationToken);
    }
}
