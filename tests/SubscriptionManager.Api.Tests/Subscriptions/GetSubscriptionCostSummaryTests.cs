using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using SubscriptionManager.Api.Tests.Authentication;
using SubscriptionManager.Application.ExchangeRates;
using SubscriptionManager.Domain.DigitalServices;
using SubscriptionManager.Domain.ExchangeRates;
using SubscriptionManager.Domain.Subscriptions;
using SubscriptionManager.Infrastructure.Identity;
using SubscriptionManager.Infrastructure.Persistence;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace SubscriptionManager.Api.Tests.Subscriptions;

public sealed class GetSubscriptionCostSummaryTests
    : IClassFixture<CustomWebApplicationFactory>
{
    private static readonly JsonSerializerOptions JsonOptions =
        new(JsonSerializerDefaults.Web)
        {
            Converters =
            {
                new JsonStringEnumConverter()
            }
        };

    private readonly CustomWebApplicationFactory _factory;

    public GetSubscriptionCostSummaryTests(
        CustomWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task GetAsync_ShouldReturnUnauthorized_WhenUserIsNotAuthenticated()
    {
        using var client =
            _factory.CreateUnauthenticatedClient();

        var response =
            await client.GetAsync(
                "/api/subscriptions/cost-summary");

        Assert.Equal(
            HttpStatusCode.Unauthorized,
            response.StatusCode);
    }

    [Fact]
    public async Task GetAsync_ShouldReturnCostsInUsersBaseCurrency()
    {
        var userId =
            Guid.NewGuid();

        await SeedAsync(
            _factory.Services,
            userId);

        using var client =
            _factory.CreateAuthenticatedClient(
                userId);

        var response =
            await client.GetAsync(
                "/api/subscriptions/cost-summary");

        Assert.Equal(
            HttpStatusCode.OK,
            response.StatusCode);

        var summary =
            await response.Content
                .ReadFromJsonAsync<CostSummaryResponse>(
                    JsonOptions);

        Assert.NotNull(summary);

        Assert.Equal(
            Currency.PLN,
            summary.BaseCurrency);

        Assert.Equal(
            new DateOnly(2026, 8, 1),
            summary.ExchangeRateEffectiveDate);

        Assert.Equal(
            2,
            summary.ActiveCount);

        Assert.Equal(
            3,
            summary.TotalCount);

        Assert.Equal(
            100m,
            summary.MonthlyCost);

        Assert.Equal(
            1200m,
            summary.YearlyCost);

        Assert.Equal(
            50m,
            summary.AverageMonthlyCost);

        Assert.Equal(
            600m,
            summary.AverageYearlyCost);

        Assert.Collection(
            summary.TopSubscriptions,
            first =>
            {
                Assert.Equal(
                    "Netflix",
                    first.Name);

                Assert.Equal(
                    60m,
                    first.MonthlyCost);
            },
            second =>
            {
                Assert.Equal(
                    "Spotify",
                    second.Name);

                Assert.Equal(
                    40m,
                    second.MonthlyCost);
            });

        Assert.Collection(
            summary.Categories,
            first =>
            {
                Assert.Equal(
                    DigitalServiceCategory.Video,
                    first.Category);

                Assert.Equal(
                    60m,
                    first.MonthlyCost);
            },
            second =>
            {
                Assert.Equal(
                    DigitalServiceCategory.Music,
                    second.Category);

                Assert.Equal(
                    40m,
                    second.MonthlyCost);
            });
    }

    [Fact]
    public async Task GetAsync_ShouldReturnServiceUnavailable_WhenExchangeRatesAreUnavailable()
    {
        await using var factory =
            _factory.WithWebHostBuilder(
                builder =>
                {
                    builder.ConfigureTestServices(
                        services =>
                        {
                            services.RemoveAll<
                                IExchangeRateService>();

                            services.AddScoped<
                                IExchangeRateService,
                                UnavailableExchangeRateService>();
                        });
                });

        var userId =
            Guid.NewGuid();

        await SeedAsync(
            factory.Services,
            userId);

        using var client =
            factory.CreateClient();

        client.DefaultRequestHeaders.Add(
            TestAuthenticationHandler.UserIdHeaderName,
            userId.ToString());

        var response =
            await client.GetAsync(
                "/api/subscriptions/cost-summary");

        Assert.Equal(
            HttpStatusCode.ServiceUnavailable,
            response.StatusCode);

        var problem =
            await response.Content
                .ReadFromJsonAsync<ProblemDetails>(
                    JsonOptions);

        Assert.NotNull(problem);

        Assert.Equal(
            StatusCodes.Status503ServiceUnavailable,
            problem.Status);

        Assert.Equal(
            "Exchange rates are unavailable.",
            problem.Title);

        Assert.Equal(
            "Subscription costs could not be converted at this time.",
            problem.Detail);
    }

    private static async Task SeedAsync(
        IServiceProvider services,
        Guid userId)
    {
        await using var scope =
            services.CreateAsyncScope();

        var dbContext =
            scope.ServiceProvider
                .GetRequiredService<
                    SubscriptionManagerDbContext>();

        dbContext.Users.Add(
            new ApplicationUser
            {
                Id = userId,
                UserName = $"{userId}@example.com",
                Email = $"{userId}@example.com",
                BaseCurrency = Currency.PLN
            });

        var netflix =
            CreateSubscription(
                userId,
                "Netflix",
                60m,
                Currency.PLN,
                BillingPeriod.Monthly,
                DigitalServiceCategory.Video);

        var spotify =
            CreateSubscription(
                userId,
                "Spotify",
                30m,
                Currency.EUR,
                BillingPeriod.Quarterly,
                DigitalServiceCategory.Music);

        var endedSubscription =
            CreateSubscription(
                userId,
                "Ended service",
                100m,
                Currency.USD,
                BillingPeriod.Monthly,
                DigitalServiceCategory.Other);

        endedSubscription.End(
            new DateOnly(2026, 7, 31));

        dbContext.Subscriptions.AddRange(
            netflix,
            spotify,
            endedSubscription);

        if (!dbContext.ExchangeRates.Any())
        {
            var checkedAt =
                DateTimeOffset.UtcNow;

            var effectiveDate =
                new DateOnly(2026, 8, 1);

            dbContext.ExchangeRates.AddRange(
                CreateExchangeRate(
                    Currency.EUR,
                    4m,
                    effectiveDate,
                    checkedAt),
                CreateExchangeRate(
                    Currency.USD,
                    3.8m,
                    effectiveDate,
                    checkedAt),
                CreateExchangeRate(
                    Currency.GBP,
                    4.9m,
                    effectiveDate,
                    checkedAt),
                CreateExchangeRate(
                    Currency.CHF,
                    4.5m,
                    effectiveDate,
                    checkedAt),
                CreateExchangeRate(
                    Currency.CZK,
                    0.175m,
                    effectiveDate,
                    checkedAt),
                CreateExchangeRate(
                    Currency.SEK,
                    0.375m,
                    effectiveDate,
                    checkedAt),
                CreateExchangeRate(
                    Currency.NOK,
                    0.365m,
                    effectiveDate,
                    checkedAt),
                CreateExchangeRate(
                    Currency.DKK,
                    0.58m,
                    effectiveDate,
                    checkedAt));
        }

        await dbContext.SaveChangesAsync();
    }

    private static Subscription CreateSubscription(
        Guid ownerId,
        string name,
        decimal amount,
        Currency currency,
        BillingPeriod billingPeriod,
        DigitalServiceCategory category)
    {
        var subscription =
            new Subscription(
                Guid.NewGuid(),
                ownerId,
                name,
                amount,
                currency,
                billingPeriod,
                new DateOnly(2026, 1, 1));

        subscription.AssignDigitalService(
            Guid.NewGuid(),
            category,
            null,
            null,
            null);

        return subscription;
    }

    private static ExchangeRate CreateExchangeRate(
        Currency currency,
        decimal rateToPln,
        DateOnly effectiveDate,
        DateTimeOffset checkedAt)
    {
        return new ExchangeRate(
            currency,
            rateToPln,
            effectiveDate,
            checkedAt);
    }

    private sealed class UnavailableExchangeRateService
        : IExchangeRateService
    {
        public Task<CurrentExchangeRates> GetCurrentAsync(
            CancellationToken cancellationToken = default)
        {
            throw new ExchangeRatesUnavailableException(
                "Current exchange rates are unavailable.");
        }
    }

    private sealed record CostSummaryResponse(
        Currency BaseCurrency,
        DateOnly? ExchangeRateEffectiveDate,
        int ActiveCount,
        int TotalCount,
        decimal MonthlyCost,
        decimal YearlyCost,
        decimal AverageMonthlyCost,
        decimal AverageYearlyCost,
        IReadOnlyCollection<CostSummaryItemResponse>
            TopSubscriptions,
        IReadOnlyCollection<CategoryCostResponse>
            Categories);

    private sealed record CostSummaryItemResponse(
        Guid Id,
        string Name,
        BillingPeriod BillingPeriod,
        decimal MonthlyCost);

    private sealed record CategoryCostResponse(
        DigitalServiceCategory Category,
        string? CustomCategoryName,
        decimal MonthlyCost);
}
