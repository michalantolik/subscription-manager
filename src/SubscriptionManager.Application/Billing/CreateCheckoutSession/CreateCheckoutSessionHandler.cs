using SubscriptionManager.Application.Common.Identity;
using SubscriptionManager.Application.Identity;

namespace SubscriptionManager.Application.Billing.CreateCheckoutSession;

public sealed class CreateCheckoutSessionHandler(
    ICurrentUser currentUser,
    IIdentityService identityService,
    IPaymentProvider paymentProvider)
{
    public async Task<Uri?> HandleAsync(
        CreateCheckoutSessionCommand command,
        CancellationToken cancellationToken = default)
    {
        var userId = currentUser.UserId;

        var email = await identityService.GetEmailAsync(
            userId,
            cancellationToken);

        if (email is null)
        {
            return null;
        }

        return await paymentProvider.CreateCheckoutSessionAsync(
            userId,
            email,
            command.Plan,
            command.BillingInterval,
            new Uri(command.SuccessUrl),
            new Uri(command.CancelUrl),
            cancellationToken);
    }
}
