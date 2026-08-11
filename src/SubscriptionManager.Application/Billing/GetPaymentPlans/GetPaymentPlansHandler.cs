namespace SubscriptionManager.Application.Billing.GetPaymentPlans;

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
