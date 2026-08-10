using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.DependencyInjection;
using SubscriptionManager.Application.Billing.GetBillingOverview;
using SubscriptionManager.Domain.Billing;
using SubscriptionManager.Infrastructure.Persistence;

namespace SubscriptionManager.Api.Tests.Billing;

public sealed class GetBillingOverviewTests
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

    public GetBillingOverviewTests(
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
                "/api/billing");

        Assert.Equal(
            HttpStatusCode.Unauthorized,
            response.StatusCode);
    }

    [Fact]
    public async Task GetAsync_ShouldReturnFreePlan_WhenSubscriptionDoesNotExist()
    {
        var userId =
            Guid.NewGuid();

        using var client =
            _factory.CreateAuthenticatedClient(
                userId);

        var response =
            await client.GetAsync(
                "/api/billing");

        Assert.Equal(
            HttpStatusCode.OK,
            response.StatusCode);

        var overview =
            await response.Content
                .ReadFromJsonAsync<BillingOverviewDto>(
                    JsonOptions);

        Assert.NotNull(
            overview);

        Assert.Equal(
            SubscriptionPlan.Free,
            overview.Plan);

        Assert.Null(
            overview.BillingInterval);

        Assert.Null(
            overview.Status);

        Assert.Null(
            overview.CurrentPeriodStart);

        Assert.Null(
            overview.CurrentPeriodEnd);

        Assert.False(
            overview.CancelAtPeriodEnd);
    }

    [Fact]
    public async Task GetAsync_ShouldReturnCurrentPaidSubscription()
    {
        var userId =
            Guid.NewGuid();

        using var client =
            _factory.CreateAuthenticatedClient(
                userId);

        var periodStart =
            new DateTimeOffset(
                2026,
                8,
                10,
                0,
                0,
                0,
                TimeSpan.Zero);

        var periodEnd =
            periodStart.AddYears(1);

        await SeedBillingSubscriptionAsync(
            userId,
            periodStart,
            periodEnd);

        var response =
            await client.GetAsync(
                "/api/billing");

        Assert.Equal(
            HttpStatusCode.OK,
            response.StatusCode);

        var overview =
            await response.Content
                .ReadFromJsonAsync<BillingOverviewDto>(
                    JsonOptions);

        Assert.NotNull(
            overview);

        Assert.Equal(
            SubscriptionPlan.Premium,
            overview.Plan);

        Assert.Equal(
            BillingInterval.Yearly,
            overview.BillingInterval);

        Assert.Equal(
            BillingSubscriptionStatus.Active,
            overview.Status);

        Assert.Equal(
            periodStart,
            overview.CurrentPeriodStart);

        Assert.Equal(
            periodEnd,
            overview.CurrentPeriodEnd);

        Assert.True(
            overview.CancelAtPeriodEnd);
    }

    private async Task SeedBillingSubscriptionAsync(
        Guid userId,
        DateTimeOffset periodStart,
        DateTimeOffset periodEnd)
    {
        await using var scope =
            _factory.Services.CreateAsyncScope();

        var dbContext =
            scope.ServiceProvider
                .GetRequiredService<
                    SubscriptionManagerDbContext>();

        var subscription =
            new BillingSubscription(
                Guid.NewGuid(),
                userId,
                SubscriptionPlan.Premium,
                BillingInterval.Yearly,
                periodStart,
                periodEnd);

        subscription.LinkToPaymentProvider(
            $"cus_{userId:N}",
            $"sub_{userId:N}",
            "price_premium_yearly");

        subscription.Synchronize(
            SubscriptionPlan.Premium,
            BillingInterval.Yearly,
            BillingSubscriptionStatus.Active,
            "price_premium_yearly",
            periodStart,
            periodEnd,
            true);

        dbContext.BillingSubscriptions.Add(
            subscription);

        await dbContext.SaveChangesAsync();
    }
}
