namespace SubscriptionManager.Application.Billing.PaymentProvider;

/// <summary>
/// Provides available payment plan prices from the payment provider.
/// </summary>
public interface IPaymentPlanCatalog
{
    Task<IReadOnlyList<PaymentPlanPrice>> GetPricesAsync(
        CancellationToken cancellationToken = default);
}
