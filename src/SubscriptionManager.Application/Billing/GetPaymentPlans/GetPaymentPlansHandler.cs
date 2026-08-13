using SubscriptionManager.Application.Billing.PaymentProvider;

namespace SubscriptionManager.Application.Billing.GetPaymentPlans;

/// <summary>
/// Handles payment plans retrieval.
/// </summary>
public sealed class GetPaymentPlansHandler(
    IPaymentPlanCatalog paymentPlanCatalog)
{
    public async Task<IReadOnlyList<PaymentPlanPrice>>
        HandleAsync(
            CancellationToken cancellationToken = default)
    {
        return await paymentPlanCatalog.GetPricesAsync(
            cancellationToken);
    }
}
