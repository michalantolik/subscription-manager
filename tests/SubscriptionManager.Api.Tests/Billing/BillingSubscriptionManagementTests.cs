using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc;
using SubscriptionManager.Domain.Billing;

namespace SubscriptionManager.Api.Tests.Billing;

public sealed class BillingSubscriptionManagementTests(
    CustomWebApplicationFactory factory)
    : IClassFixture<CustomWebApplicationFactory>
{
    [Fact]
    public async Task PreviewChangeAsync_ShouldReturnConflict_WhenSubscriptionDoesNotExist()
    {
        using var client =
            factory.CreateClient();

        var response =
            await client.PostAsJsonAsync(
                "/api/billing/subscription/change-preview",
                new
                {
                    Plan =
                        SubscriptionPlan.Premium,
                    BillingInterval =
                        BillingInterval.Monthly
                });

        Assert.Equal(
            HttpStatusCode.Conflict,
            response.StatusCode);

        var problemDetails =
            await response.Content.ReadFromJsonAsync<
                ProblemDetails>();

        Assert.NotNull(
            problemDetails);

        Assert.Equal(
            "Subscription change is unavailable.",
            problemDetails.Title);
    }

    [Fact]
    public async Task ChangeAsync_ShouldReturnConflict_WhenSubscriptionDoesNotExist()
    {
        using var client =
            factory.CreateClient();

        var response =
            await client.PostAsJsonAsync(
                "/api/billing/subscription/change",
                new
                {
                    Plan =
                        SubscriptionPlan.Premium,
                    BillingInterval =
                        BillingInterval.Monthly
                });

        Assert.Equal(
            HttpStatusCode.Conflict,
            response.StatusCode);

        var problemDetails =
            await response.Content.ReadFromJsonAsync<
                ProblemDetails>();

        Assert.NotNull(
            problemDetails);

        Assert.Equal(
            "Subscription change is unavailable.",
            problemDetails.Title);
    }

    [Fact]
    public async Task CancelAsync_ShouldReturnConflict_WhenSubscriptionDoesNotExist()
    {
        using var client =
            factory.CreateClient();

        var response =
            await client.PostAsync(
                "/api/billing/subscription/cancel",
                content: null);

        Assert.Equal(
            HttpStatusCode.Conflict,
            response.StatusCode);

        var problemDetails =
            await response.Content.ReadFromJsonAsync<
                ProblemDetails>();

        Assert.NotNull(
            problemDetails);

        Assert.Equal(
            "Subscription cancellation is unavailable.",
            problemDetails.Title);
    }

    [Fact]
    public async Task ResumeAsync_ShouldReturnConflict_WhenSubscriptionDoesNotExist()
    {
        using var client =
            factory.CreateClient();

        var response =
            await client.PostAsync(
                "/api/billing/subscription/resume",
                content: null);

        Assert.Equal(
            HttpStatusCode.Conflict,
            response.StatusCode);

        var problemDetails =
            await response.Content.ReadFromJsonAsync<
                ProblemDetails>();

        Assert.NotNull(
            problemDetails);

        Assert.Equal(
            "Subscription renewal cannot be resumed.",
            problemDetails.Title);
    }

    [Theory]
    [InlineData(
        "/api/billing/subscription/change-preview",
        true)]
    [InlineData(
        "/api/billing/subscription/change",
        true)]
    [InlineData(
        "/api/billing/subscription/cancel",
        false)]
    [InlineData(
        "/api/billing/subscription/resume",
        false)]
    public async Task Endpoint_ShouldReturnUnauthorized_WhenUserIsNotAuthenticated(
        string requestUri,
        bool requiresBody)
    {
        using var client =
            factory.CreateUnauthenticatedClient();

        HttpResponseMessage response;

        if (requiresBody)
        {
            response =
                await client.PostAsJsonAsync(
                    requestUri,
                    new
                    {
                        Plan =
                            SubscriptionPlan.Premium,
                        BillingInterval =
                            BillingInterval.Monthly
                    });
        }
        else
        {
            response =
                await client.PostAsync(
                    requestUri,
                    content: null);
        }

        Assert.Equal(
            HttpStatusCode.Unauthorized,
            response.StatusCode);
    }
}
