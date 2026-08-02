using System.Net;
using System.Text;
using SubscriptionManager.Domain.Subscriptions;
using SubscriptionManager.Infrastructure.ExchangeRates;

namespace SubscriptionManager.Infrastructure.Tests.ExchangeRates;

public sealed class NbpExchangeRateProviderTests
{
    [Fact]
    public async Task GetLatestAsync_ShouldReturnSupportedExchangeRates()
    {
        Uri? requestedUri = null;

        using var httpClient =
            CreateHttpClient(request =>
            {
                requestedUri = request.RequestUri;

                return CreateJsonResponse(
                    CompleteTableJson);
            });

        var provider =
            new NbpExchangeRateProvider(
                httpClient);

        var result =
            await provider.GetLatestAsync();

        Assert.Equal(
            new DateOnly(2026, 8, 1),
            result.EffectiveDate);

        Assert.Equal(
            8,
            result.Rates.Count);

        Assert.DoesNotContain(
            result.Rates,
            rate =>
                rate.Currency == Currency.PLN);

        Assert.DoesNotContain(
            result.Rates,
            rate =>
                !Enum.IsDefined(rate.Currency));

        Assert.Equal(
            4.3m,
            result.Rates.Single(rate =>
                rate.Currency == Currency.EUR)
                .RateToPln);

        Assert.Equal(
            4.9m,
            result.Rates.Single(rate =>
                rate.Currency == Currency.GBP)
                .RateToPln);

        Assert.Equal(
            "https://api.nbp.pl/api/exchangerates/tables/A?format=json",
            requestedUri?.ToString());
    }

    [Fact]
    public async Task GetLatestAsync_ShouldThrow_WhenSupportedCurrencyIsMissing()
    {
        const string incompleteTableJson =
            """
            [
              {
                "table": "A",
                "no": "146/A/NBP/2026",
                "effectiveDate": "2026-08-01",
                "rates": [
                  { "currency": "euro", "code": "EUR", "mid": 4.300000 },
                  { "currency": "dolar amerykański", "code": "USD", "mid": 3.900000 },
                  { "currency": "funt szterling", "code": "GBP", "mid": 4.900000 },
                  { "currency": "frank szwajcarski", "code": "CHF", "mid": 4.500000 },
                  { "currency": "korona czeska", "code": "CZK", "mid": 0.175000 },
                  { "currency": "korona szwedzka", "code": "SEK", "mid": 0.375000 },
                  { "currency": "korona norweska", "code": "NOK", "mid": 0.365000 }
                ]
              }
            ]
            """;

        using var httpClient =
            CreateHttpClient(_ =>
                CreateJsonResponse(
                    incompleteTableJson));

        var provider =
            new NbpExchangeRateProvider(
                httpClient);

        var exception =
            await Assert.ThrowsAsync<InvalidOperationException>(
                () => provider.GetLatestAsync());

        Assert.Contains(
            "DKK",
            exception.Message);
    }

    [Fact]
    public async Task GetLatestAsync_ShouldThrow_WhenNbpRequestFails()
    {
        using var httpClient =
            CreateHttpClient(_ =>
                new HttpResponseMessage(
                    HttpStatusCode.ServiceUnavailable));

        var provider =
            new NbpExchangeRateProvider(
                httpClient);

        await Assert.ThrowsAsync<HttpRequestException>(
            () => provider.GetLatestAsync());
    }

    private static HttpClient CreateHttpClient(
        Func<HttpRequestMessage, HttpResponseMessage> responseFactory)
    {
        var handler =
            new StubHttpMessageHandler(
                responseFactory);

        return new HttpClient(handler)
        {
            BaseAddress =
                new Uri("https://api.nbp.pl/")
        };
    }

    private static HttpResponseMessage CreateJsonResponse(
        string json)
    {
        return new HttpResponseMessage(
            HttpStatusCode.OK)
        {
            Content = new StringContent(
                json,
                Encoding.UTF8,
                "application/json")
        };
    }

    private const string CompleteTableJson =
        """
        [
          {
            "table": "A",
            "no": "146/A/NBP/2026",
            "effectiveDate": "2026-08-01",
            "rates": [
              { "currency": "euro", "code": "EUR", "mid": 4.300000 },
              { "currency": "dolar amerykański", "code": "USD", "mid": 3.900000 },
              { "currency": "funt szterling", "code": "GBP", "mid": 4.900000 },
              { "currency": "frank szwajcarski", "code": "CHF", "mid": 4.500000 },
              { "currency": "korona czeska", "code": "CZK", "mid": 0.175000 },
              { "currency": "korona szwedzka", "code": "SEK", "mid": 0.375000 },
              { "currency": "korona norweska", "code": "NOK", "mid": 0.365000 },
              { "currency": "korona duńska", "code": "DKK", "mid": 0.580000 },
              { "currency": "jen", "code": "JPY", "mid": 0.026000 }
            ]
          }
        ]
        """;

    private sealed class StubHttpMessageHandler(
        Func<HttpRequestMessage, HttpResponseMessage> responseFactory)
        : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            return Task.FromResult(
                responseFactory(request));
        }
    }
}
