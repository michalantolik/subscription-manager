using SubscriptionManager.Web.Common.Currencies;
using SubscriptionManager.Web.Features.SavingsPlans;
using System.Net;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text.Json;
using SubscriptionManager.Web.Features.Authentication.Security;

namespace SubscriptionManager.Web.Tests;

public sealed class SavingsPlansApiClientTests
{
    [Fact]
    public async Task GetUsageAsync_ShouldSendAuthorizedRequestAndReturnUsage()
    {
        HttpRequestMessage? capturedRequest = null;

        using var httpClient =
            new HttpClient(
                new StubHttpMessageHandler(
                    (
                        request,
                        _) =>
                    {
                        capturedRequest = request;

                        return Task.FromResult(
                            new HttpResponseMessage(
                                HttpStatusCode.OK)
                            {
                                Content = JsonContent.Create(
                                    new
                                    {
                                        SubscriptionPlan = "Free",
                                        DailyRequestLimit = 3,
                                        RemainingRequestCount = 2
                                    })
                            });
                    }))
            {
                BaseAddress =
                    new Uri("https://api.example.com")
            };

        var apiClient =
            new SavingsPlansApiClient(
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
            await apiClient.GetUsageAsync(
                user);

        Assert.NotNull(
            capturedRequest);

        Assert.Equal(
            HttpMethod.Get,
            capturedRequest.Method);

        Assert.Equal(
            "https://api.example.com/api/savings-plans/usage",
            capturedRequest.RequestUri?.ToString());

        Assert.Equal(
            "Bearer",
            capturedRequest.Headers.Authorization?.Scheme);

        Assert.Equal(
            "access-token",
            capturedRequest.Headers.Authorization?.Parameter);

        Assert.Equal(
            SubscriptionPlan.Free,
            result.SubscriptionPlan);

        Assert.Equal(
            3,
            result.DailyRequestLimit);

        Assert.Equal(
            2,
            result.RemainingRequestCount);
    }

    [Fact]
    public async Task CreateAsync_ShouldSendAuthorizedRequestAndReturnPlan()
    {
        HttpRequestMessage? capturedRequest = null;
        string? capturedContent = null;

        var protectedSubscriptionId =
            Guid.NewGuid();

        var recommendedSubscriptionId =
            Guid.NewGuid();

        using var httpClient =
            new HttpClient(
                new StubHttpMessageHandler(
                    async (
                        request,
                        cancellationToken) =>
                    {
                        capturedRequest = request;

                        capturedContent =
                            await request.Content!
                                .ReadAsStringAsync(
                                    cancellationToken);

                        return new HttpResponseMessage(
                            HttpStatusCode.OK)
                        {
                            Content = JsonContent.Create(
                                new
                                {
                                    BaseCurrency = "PLN",
                                    CurrentMonthlyCost = 100m,
                                    Recommended = new
                                    {
                                        Subscriptions = new[]
                                        {
                                            new
                                            {
                                                Id =
                                                    recommendedSubscriptionId,
                                                Name = "Netflix",
                                                Category = "Video",
                                                MonthlyCost = 60m
                                            }
                                        },
                                        ProjectedMonthlyCost = 40m,
                                        MonthlySavings = 60m,
                                        YearlySavings = 720m,
                                        TargetReached = true,
                                        Explanation =
                                            "The selected scenario reaches the budget."
                                    },
                                    Alternative = (object?)null,
                                    SubscriptionPlan = "Free",
                                    DailyRequestLimit = 3,
                                    RemainingRequestCount = 2
                                })
                        };
                    }))
            {
                BaseAddress =
                    new Uri("https://api.example.com")
            };

        var apiClient =
            new SavingsPlansApiClient(
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

        var request =
            new CreateSavingsPlanRequest(
                SavingsPlanGoalType.MonthlyBudget,
                50m,
                [protectedSubscriptionId],
                SavingsPlanStrategy.Balanced,
                "Keep music services.",
                "pl");

        var result =
            await apiClient.CreateAsync(
                request,
                user);

        Assert.NotNull(
            capturedRequest);

        Assert.Equal(
            HttpMethod.Post,
            capturedRequest.Method);

        Assert.Equal(
            "https://api.example.com/api/savings-plans",
            capturedRequest.RequestUri?.ToString());

        Assert.Equal(
            "Bearer",
            capturedRequest.Headers.Authorization?.Scheme);

        Assert.Equal(
            "access-token",
            capturedRequest.Headers.Authorization?.Parameter);

        Assert.NotNull(
            capturedContent);

        using var document =
            JsonDocument.Parse(
                capturedContent);

        var root =
            document.RootElement;

        Assert.Equal(
            "MonthlyBudget",
            root.GetProperty("goalType").GetString());

        Assert.Equal(
            50m,
            root.GetProperty("targetAmount").GetDecimal());

        Assert.Equal(
            protectedSubscriptionId,
            root.GetProperty("protectedSubscriptionIds")[0]
                .GetGuid());

        Assert.Equal(
            "Balanced",
            root.GetProperty("strategy").GetString());

        Assert.Equal(
            "Keep music services.",
            root.GetProperty("additionalPreference")
                .GetString());

        Assert.Equal(
            "pl",
            root.GetProperty("languageCode").GetString());

        Assert.Equal(
            Currency.PLN,
            result.BaseCurrency);

        Assert.Equal(
            100m,
            result.CurrentMonthlyCost);

        Assert.Equal(
            SubscriptionPlan.Free,
            result.SubscriptionPlan);

        Assert.Equal(
            3,
            result.DailyRequestLimit);

        Assert.Equal(
            2,
            result.RemainingRequestCount);

        var recommended =
            Assert.IsType<SavingsPlanScenarioResponse>(
                result.Recommended);

        Assert.Equal(
            40m,
            recommended.ProjectedMonthlyCost);

        Assert.Equal(
            60m,
            recommended.MonthlySavings);

        Assert.Equal(
            720m,
            recommended.YearlySavings);

        Assert.True(
            recommended.TargetReached);

        var subscription =
            Assert.Single(
                recommended.Subscriptions);

        Assert.Equal(
            recommendedSubscriptionId,
            subscription.Id);

        Assert.Equal(
            "Netflix",
            subscription.Name);

        Assert.Null(
            result.Alternative);
    }

    [Fact]
    public async Task CreateAsync_ShouldThrowUsageLimitExceededException_WhenDailyLimitIsReached()
    {
        using var httpClient =
            new HttpClient(
                new StubHttpMessageHandler(
                    (_, _) =>
                        Task.FromResult(
                            new HttpResponseMessage(
                                HttpStatusCode.TooManyRequests)
                            {
                                Content = JsonContent.Create(
                                    new
                                    {
                                        title =
                                            "Daily savings plan limit reached.",
                                        detail =
                                            "The daily savings plan limit of 3 requests has been reached.",
                                        dailyLimit = 3
                                    })
                            })))
            {
                BaseAddress =
                    new Uri("https://api.example.com")
            };

        var apiClient =
            new SavingsPlansApiClient(
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

        var exception =
            await Assert.ThrowsAsync<
                SavingsPlanUsageLimitExceededException>(
                () =>
                    apiClient.CreateAsync(
                        new CreateSavingsPlanRequest(
                            SavingsPlanGoalType.MonthlyBudget,
                            50m,
                            [],
                            SavingsPlanStrategy.Balanced,
                            null,
                            "en"),
                        user));

        Assert.Equal(
            "The daily savings plan limit of 3 requests has been reached.",
            exception.Message);

        Assert.Equal(
            3,
            exception.DailyLimit);
    }

    private sealed class StubHttpMessageHandler(
        Func<
            HttpRequestMessage,
            CancellationToken,
            Task<HttpResponseMessage>> responseFactory)
        : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            return responseFactory(
                request,
                cancellationToken);
        }
    }
}
