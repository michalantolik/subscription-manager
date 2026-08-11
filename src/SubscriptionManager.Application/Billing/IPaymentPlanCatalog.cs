namespace SubscriptionManager.Application.Billing;

public interface IPaymentPlanCatalog
{
    Task<IReadOnlyList<PaymentPlanPrice>> GetPricesAsync(
        CancellationToken cancellationToken = default);
}
