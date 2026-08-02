using SubscriptionManager.Domain.ExchangeRates;
using SubscriptionManager.Domain.Subscriptions;

namespace SubscriptionManager.Application.ExchangeRates;

public sealed class ExchangeRateService(
    IExchangeRateRepository exchangeRateRepository,
    IExchangeRateProvider exchangeRateProvider,
    TimeProvider timeProvider)
    : IExchangeRateService
{
    private static readonly Currency[] ForeignCurrencies =
        Enum.GetValues<Currency>()
            .Where(currency =>
                currency != Currency.PLN)
            .ToArray();

    public async Task<CurrentExchangeRates> GetCurrentAsync(
        CancellationToken cancellationToken = default)
    {
        var storedRates =
            (await exchangeRateRepository.GetAllAsync(
                cancellationToken))
            .ToList();

        var checkedAt =
            timeProvider.GetUtcNow();

        var currentDate =
            DateOnly.FromDateTime(
                checkedAt.UtcDateTime);

        if (ContainsAllCurrencies(storedRates) &&
            WereCheckedOn(
                storedRates,
                currentDate))
        {
            return CreateResult(storedRates);
        }

        ExchangeRateSnapshot snapshot;

        try
        {
            snapshot =
                await exchangeRateProvider.GetLatestAsync(
                    cancellationToken);

            ValidateSnapshot(snapshot);
        }
        catch (OperationCanceledException)
            when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception exception)
        {
            return await UseStoredRatesAsync(
                storedRates,
                checkedAt,
                exception,
                cancellationToken);
        }

        await UpdateStoredRatesAsync(
            storedRates,
            snapshot,
            checkedAt,
            cancellationToken);

        return CreateResult(storedRates);
    }

    private async Task<CurrentExchangeRates> UseStoredRatesAsync(
        IReadOnlyCollection<ExchangeRate> storedRates,
        DateTimeOffset checkedAt,
        Exception providerException,
        CancellationToken cancellationToken)
    {
        if (!ContainsAllCurrencies(storedRates))
        {
            throw new ExchangeRatesUnavailableException(
                "Current exchange rates are unavailable.",
                providerException);
        }

        foreach (var rate in storedRates)
        {
            rate.MarkAsChecked(checkedAt);
        }

        await exchangeRateRepository.SaveChangesAsync(
            cancellationToken);

        return CreateResult(storedRates);
    }

    private async Task UpdateStoredRatesAsync(
        ICollection<ExchangeRate> storedRates,
        ExchangeRateSnapshot snapshot,
        DateTimeOffset checkedAt,
        CancellationToken cancellationToken)
    {
        var quotes =
            snapshot.Rates.ToDictionary(
                quote => quote.Currency);

        var storedRatesByCurrency =
            storedRates.ToDictionary(
                rate => rate.Currency);

        var newRates =
            new List<ExchangeRate>();

        foreach (var currency in ForeignCurrencies)
        {
            var quote = quotes[currency];

            if (storedRatesByCurrency.TryGetValue(
                    currency,
                    out var storedRate))
            {
                storedRate.Update(
                    quote.RateToPln,
                    snapshot.EffectiveDate,
                    checkedAt);

                continue;
            }

            var newRate =
                new ExchangeRate(
                    currency,
                    quote.RateToPln,
                    snapshot.EffectiveDate,
                    checkedAt);

            newRates.Add(newRate);
            storedRates.Add(newRate);
        }

        if (newRates.Count > 0)
        {
            await exchangeRateRepository.AddRangeAsync(
                newRates,
                cancellationToken);
        }

        await exchangeRateRepository.SaveChangesAsync(
            cancellationToken);
    }

    private static void ValidateSnapshot(
        ExchangeRateSnapshot snapshot)
    {
        if (snapshot.EffectiveDate == default)
        {
            throw new InvalidOperationException(
                "The exchange rate effective date is unavailable.");
        }

        var quotes =
            snapshot.Rates
                .Where(quote =>
                    quote.Currency != Currency.PLN)
                .ToDictionary(
                    quote => quote.Currency);

        foreach (var currency in ForeignCurrencies)
        {
            if (!quotes.TryGetValue(
                    currency,
                    out var quote))
            {
                throw new InvalidOperationException(
                    $"The exchange rate for {currency} is unavailable.");
            }

            if (quote.RateToPln <= 0)
            {
                throw new InvalidOperationException(
                    $"The exchange rate for {currency} is invalid.");
            }
        }
    }

    private static bool ContainsAllCurrencies(
        IReadOnlyCollection<ExchangeRate> rates)
    {
        return ForeignCurrencies.All(currency =>
            rates.Any(rate =>
                rate.Currency == currency));
    }

    private static bool WereCheckedOn(
        IEnumerable<ExchangeRate> rates,
        DateOnly date)
    {
        return rates.All(rate =>
            DateOnly.FromDateTime(
                rate.LastCheckedAt.UtcDateTime) ==
            date);
    }

    private static CurrentExchangeRates CreateResult(
        IReadOnlyCollection<ExchangeRate> rates)
    {
        if (!ContainsAllCurrencies(rates))
        {
            throw new ExchangeRatesUnavailableException(
                "The stored exchange rates are incomplete.");
        }

        var ratesToPln =
            rates.ToDictionary(
                rate => rate.Currency,
                rate => rate.RateToPln);

        ratesToPln[Currency.PLN] = 1m;

        var effectiveDate =
            rates.Min(rate =>
                rate.EffectiveDate);

        return new CurrentExchangeRates(
            effectiveDate,
            ratesToPln);
    }
}

public sealed class ExchangeRatesUnavailableException
    : Exception
{
    public ExchangeRatesUnavailableException(
        string message)
        : base(message)
    {
    }

    public ExchangeRatesUnavailableException(
        string message,
        Exception innerException)
        : base(message, innerException)
    {
    }
}
