using System.Net;
using System.Net.Http.Json;
using System.Security.Claims;
using SubscriptionManager.Blazor.Features.Authentication;
using SubscriptionManager.Blazor.Features.Currencies;
using SubscriptionManager.Blazor.Features.Subscriptions;

namespace SubscriptionManager.Blazor.Tests;

public sealed class SubscriptionsApiClientTests
{
    [Fact]
    public async Task GetCostSummaryAsync_SendsAuthorizedRequestAndReturnsSummary()
    {
        HttpRequestMessage? capturedRequest = null;

        var effectiveDate =
            new DateOnly(2026, 8, 1);

        using var httpClient =
            new HttpClient(
                new StubHttpMessageHandler(request =>
                {
                    capturedRequest = request;

                    return new HttpResponseMessage(
                        HttpStatusCode.OK)
                    {
                        Content = JsonContent.Create(
                            new
                            {
                                BaseCurrency = "EUR",
                                ExchangeRateEffectiveDate =
                                    effectiveDate,
                                ActiveCount = 2,
                                TotalCount = 3,
                                MonthlyCost = 25m,
                                YearlyCost = 300m,
                                AverageMonthlyCost = 12.5m,
                                AverageYearlyCost = 150m,
                                TopSubscriptions = new[]
                                {
                                    new
                                    {
                                        Id = Guid.NewGuid(),
                                        Name = "Netflix",
                                        BillingPeriod = "Monthly",
                                        MonthlyCost = 15m
                                    }
                                },
                                Categories = new[]
                                {
                                    new
                                    {
                                        Category = "Video",
                                        CustomCategoryName =
                                            (string?)null,
                                        MonthlyCost = 15m
                                    }
                                }
                            })
                    };
                }))
            {
                BaseAddress =
                    new Uri("https://api.example.com")
            };

        var apiClient =
            new SubscriptionsApiClient(
                httpClient);

        var user =
            new ClaimsPrincipal(
                new ClaimsIdentity(
                [
                    new Claim(
                        AuthenticationClaimTypes.AccessToken,
                        "access-token")
                ],
                "Test"));

        var result =
            await apiClient.GetCostSummaryAsync(
                user);

        Assert.NotNull(capturedRequest);

        Assert.Equal(
            HttpMethod.Get,
            capturedRequest.Method);

        Assert.Equal(
            "https://api.example.com/api/subscriptions/cost-summary",
            capturedRequest.RequestUri?.ToString());

        Assert.Equal(
            "Bearer",
            capturedRequest.Headers.Authorization?.Scheme);

        Assert.Equal(
            "access-token",
            capturedRequest.Headers.Authorization?.Parameter);

        Assert.Equal(
            Currency.EUR,
            result.BaseCurrency);

        Assert.Equal(
            effectiveDate,
            result.ExchangeRateEffectiveDate);

        Assert.Equal(
            2,
            result.ActiveCount);

        Assert.Equal(
            25m,
            result.MonthlyCost);

        var topSubscription =
            Assert.Single(
                result.TopSubscriptions);

        Assert.Equal(
            "Netflix",
            topSubscription.Name);

        Assert.Equal(
            BillingPeriod.Monthly,
            topSubscription.BillingPeriod);

        var category =
            Assert.Single(
                result.Categories);

        Assert.Equal(
            "Video",
            category.Category);

        Assert.Equal(
            15m,
            category.MonthlyCost);
    }

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
