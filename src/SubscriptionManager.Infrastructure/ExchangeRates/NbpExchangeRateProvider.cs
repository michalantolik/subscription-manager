using System.Net.Http.Json;
using SubscriptionManager.Application.ExchangeRates;
using SubscriptionManager.Domain.Subscriptions;

namespace SubscriptionManager.Infrastructure.ExchangeRates;

internal sealed class NbpExchangeRateProvider(
    HttpClient httpClient)
    : IExchangeRateProvider
{
    private const string LatestTablePath =
        "api/exchangerates/tables/A?format=json";

    public async Task<ExchangeRateSnapshot> GetLatestAsync(
        CancellationToken cancellationToken = default)
    {
        var tables =
            await httpClient.GetFromJsonAsync<NbpExchangeRateTable[]>(
                LatestTablePath,
                cancellationToken);

        var table =
            tables?.SingleOrDefault()
            ?? throw new InvalidOperationException(
                "NBP did not return an exchange rate table.");

        if (table.EffectiveDate == default)
        {
            throw new InvalidOperationException(
                "NBP returned an exchange rate table without an effective date.");
        }

        var supportedCurrencies =
            Enum.GetValues<Currency>()
                .Where(currency =>
                    currency != Currency.PLN)
                .ToArray();

        var ratesByCurrency = table.Rates
            .Select(rate =>
            {
                var parsed =
                    Enum.TryParse<Currency>(
                        rate.Code,
                        ignoreCase: true,
                        out var currency);

                return new
                {
                    Parsed = parsed &&
                             Enum.IsDefined(currency),
                    Currency = currency,
                    rate.Mid
                };
            })
            .Where(rate =>
                rate.Parsed &&
                rate.Currency != Currency.PLN)
            .ToDictionary(
                rate => rate.Currency,
                rate => rate.Mid);

        var quotes =
            supportedCurrencies
                .Select(currency =>
                {
                    if (!ratesByCurrency.TryGetValue(
                            currency,
                            out var rateToPln))
                    {
                        throw new InvalidOperationException(
                            $"NBP did not return a rate for {currency}.");
                    }

                    if (rateToPln <= 0)
                    {
                        throw new InvalidOperationException(
                            $"NBP returned an invalid rate for {currency}.");
                    }

                    return new ExchangeRateQuote(
                        currency,
                        rateToPln);
                })
                .ToArray();

        return new ExchangeRateSnapshot(
            table.EffectiveDate,
            quotes);
    }

    private sealed record NbpExchangeRateTable(
        DateOnly EffectiveDate,
        IReadOnlyCollection<NbpExchangeRate> Rates);

    private sealed record NbpExchangeRate(
        string Code,
        decimal Mid);
}
