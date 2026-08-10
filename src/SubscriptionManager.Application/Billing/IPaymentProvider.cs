using SubscriptionManager.Domain.Billing;

namespace SubscriptionManager.Application.Billing;

public interface IPaymentProvider
{
    Task<Uri> CreateCheckoutSessionAsync(
        Guid userId,
        string email,
        SubscriptionPlan plan,
        BillingInterval billingInterval,
        Uri successUrl,
        Uri cancelUrl,
        CancellationToken cancellationToken = default);
}
