using SubscriptionManager.Blazor.Features.Authentication;
using SubscriptionManager.Blazor.Features.Currencies;
using SubscriptionManager.Blazor.Features.SavingsPlans;
using System.Net;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text.Json;

namespace SubscriptionManager.Blazor.Tests;

public sealed class SavingsPlansApiClientTests
{
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
                                    Alternative = (object?)null
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
