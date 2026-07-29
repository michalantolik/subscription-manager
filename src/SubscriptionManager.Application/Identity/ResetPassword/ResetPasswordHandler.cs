using SubscriptionManager.Application.Common.Identity;

namespace SubscriptionManager.Application.Identity.ResetPassword;

public sealed class ResetPasswordHandler(
    IIdentityService identityService)
{
    public async Task<ResetPasswordResult> HandleAsync(
        ResetPasswordCommand command,
        CancellationToken cancellationToken = default)
    {
        return await identityService.ResetPasswordAsync(
            command.UserId,
            command.ResetToken,
            command.NewPassword,
            cancellationToken);
    }
}
