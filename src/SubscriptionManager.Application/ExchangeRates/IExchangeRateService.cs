using SubscriptionManager.Domain.Subscriptions;

namespace SubscriptionManager.Application.ExchangeRates;

public interface IExchangeRateService
{
    Task<CurrentExchangeRates> GetCurrentAsync(
        CancellationToken cancellationToken = default);
}

public sealed record CurrentExchangeRates(
    DateOnly EffectiveDate,
    IReadOnlyDictionary<Currency, decimal> RatesToPln)
{
    public decimal Convert(
        decimal amount,
        Currency sourceCurrency,
        Currency targetCurrency)
    {
        if (amount < 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(amount),
                "The amount cannot be negative.");
        }

        if (!RatesToPln.TryGetValue(
                sourceCurrency,
                out var sourceRate))
        {
            throw new InvalidOperationException(
                $"The exchange rate for {sourceCurrency} is unavailable.");
        }

        if (!RatesToPln.TryGetValue(
                targetCurrency,
                out var targetRate))
        {
            throw new InvalidOperationException(
                $"The exchange rate for {targetCurrency} is unavailable.");
        }

        return amount *
               sourceRate /
               targetRate;
    }
}
