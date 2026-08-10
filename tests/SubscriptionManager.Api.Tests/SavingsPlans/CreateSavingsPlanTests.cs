using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using SubscriptionManager.Api.Tests.Authentication;
using SubscriptionManager.Application.Common.Identity;
using SubscriptionManager.Application.SavingsPlans;
using SubscriptionManager.Application.SavingsPlans.CreateSavingsPlan;
using SubscriptionManager.Domain.Billing;
using SubscriptionManager.Domain.DigitalServices;
using SubscriptionManager.Domain.Subscriptions;
using SubscriptionManager.Infrastructure.Identity;
using SubscriptionManager.Infrastructure.Persistence;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace SubscriptionManager.Api.Tests.SavingsPlans;

public sealed class CreateSavingsPlanTests
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

    public CreateSavingsPlanTests(
        CustomWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task PostAsync_ShouldReturnUnauthorized_WhenUserIsNotAuthenticated()
    {
        using var client =
            _factory.CreateUnauthenticatedClient();

        var request =
            CreateRequest();

        var response =
            await client.PostAsJsonAsync(
                "/api/savings-plans",
                request,
                JsonOptions);

        Assert.Equal(
            HttpStatusCode.Unauthorized,
            response.StatusCode);
    }

    [Fact]
    public async Task PostAsync_ShouldReturnForbidden_WhenPaidPlanIsRequired()
    {
        await using var factory =
            _factory.WithWebHostBuilder(
                builder =>
                {
                    builder.ConfigureAppConfiguration(
                        (_, configuration) =>
                        {
                            configuration.AddInMemoryCollection(
                                new Dictionary<string, string?>
                                {
                                    ["SavingsPlanAi:ApiKey"] =
                                        string.Empty
                                });
                        });
                });

        var userId =
            Guid.NewGuid();

        await SeedAsync(
            factory.Services,
            userId,
            SubscriptionPlan.Free);

        using var client =
            factory.CreateClient();

        client.DefaultRequestHeaders.Add(
            TestAuthenticationHandler.UserIdHeaderName,
            userId.ToString());

        var response =
            await client.PostAsJsonAsync(
                "/api/savings-plans",
                CreateRequest(),
                JsonOptions);

        Assert.Equal(
            HttpStatusCode.Forbidden,
            response.StatusCode);

        var problem =
            await response.Content
                .ReadFromJsonAsync<ProblemDetails>(
                    JsonOptions);

        Assert.NotNull(problem);

        Assert.Equal(
            StatusCodes.Status403Forbidden,
            problem.Status);

        Assert.Equal(
            "Savings plan access required.",
            problem.Title);

        var hasCode =
            problem.Extensions.TryGetValue(
                "code",
                out var codeValue);

        Assert.True(hasCode);

        var code =
            Assert.IsType<JsonElement>(
                codeValue);

        Assert.Equal(
            "savings_plan_access_required",
            code.GetString());
    }

    [Fact]
    public async Task PostAsync_ShouldReturnCalculatedSavingsPlan()
    {
        await using var factory =
            _factory.WithWebHostBuilder(
                builder =>
                {
                    builder.ConfigureTestServices(
                        services =>
                        {
                            services.RemoveAll<
                                ISavingsPlanAgent>();

                            services.AddScoped<
                                ISavingsPlanAgent,
                                SuccessfulSavingsPlanAgent>();
                        });
                });

        var userId =
            Guid.NewGuid();

        await SeedAsync(
            factory.Services,
            userId,
            SubscriptionPlan.Plus);

        using var client =
            factory.CreateClient();

        client.DefaultRequestHeaders.Add(
            TestAuthenticationHandler.UserIdHeaderName,
            userId.ToString());

        var response =
            await client.PostAsJsonAsync(
                "/api/savings-plans",
                CreateRequest(),
                JsonOptions);

        Assert.Equal(
            HttpStatusCode.OK,
            response.StatusCode);

        var plan =
            await response.Content
                .ReadFromJsonAsync<SavingsPlanDto>(
                    JsonOptions);

        Assert.NotNull(plan);

        Assert.Equal(
            Currency.PLN,
            plan.BaseCurrency);

        Assert.Equal(
            100m,
            plan.CurrentMonthlyCost);

        Assert.Equal(
            SubscriptionPlan.Plus,
            plan.SubscriptionPlan);

        Assert.Equal(
            SubscriptionPlanLimits
                .PlusDailySavingsPlanLimit,
            plan.DailyRequestLimit);

        var recommended =
            Assert.IsType<SavingsPlanScenarioDto>(
                plan.Recommended);

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
            "Netflix",
            subscription.Name);

        Assert.Equal(
            60m,
            subscription.MonthlyCost);

        Assert.Null(
            plan.Alternative);
    }

    [Fact]
    public async Task PostAsync_ShouldReturnServiceUnavailable_WhenAgentIsUnavailable()
    {
        await using var factory =
            _factory.WithWebHostBuilder(
                builder =>
                {
                    builder.ConfigureTestServices(
                        services =>
                        {
                            services.RemoveAll<
                                ISavingsPlanAgent>();

                            services.AddScoped<
                                ISavingsPlanAgent,
                                UnavailableSavingsPlanAgent>();
                        });
                });

        var userId =
            Guid.NewGuid();

        await SeedAsync(
            factory.Services,
            userId,
            SubscriptionPlan.Plus);

        using var client =
            factory.CreateClient();

        client.DefaultRequestHeaders.Add(
            TestAuthenticationHandler.UserIdHeaderName,
            userId.ToString());

        var response =
            await client.PostAsJsonAsync(
                "/api/savings-plans",
                CreateRequest(),
                JsonOptions);

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
            "Savings plan is unavailable.",
            problem.Title);

        Assert.Equal(
            "The savings plan could not be generated at this time. Please try again later.",
            problem.Detail);
    }

    [Fact]
    public async Task PostAsync_ShouldReturnTooManyRequests_WhenDailyLimitIsReached()
    {
        await using var factory =
            _factory.WithWebHostBuilder(
                builder =>
                {
                    builder.ConfigureTestServices(
                        services =>
                        {
                            services.RemoveAll<
                                ISavingsPlanAgent>();

                            services.AddScoped<
                                ISavingsPlanAgent,
                                SuccessfulSavingsPlanAgent>();
                        });
                });

        var userId =
            Guid.NewGuid();

        await SeedAsync(
            factory.Services,
            userId,
            SubscriptionPlan.Plus);

        await SeedUsageAsync(
            factory.Services,
            userId,
            SubscriptionPlanLimits
                .PlusDailySavingsPlanLimit);

        using var client =
            factory.CreateClient();

        client.DefaultRequestHeaders.Add(
            TestAuthenticationHandler.UserIdHeaderName,
            userId.ToString());

        var response =
            await client.PostAsJsonAsync(
                "/api/savings-plans",
                CreateRequest(),
                JsonOptions);

        Assert.Equal(
            HttpStatusCode.TooManyRequests,
            response.StatusCode);

        var problem =
            await response.Content
                .ReadFromJsonAsync<ProblemDetails>(
                    JsonOptions);

        Assert.NotNull(problem);

        Assert.Equal(
            StatusCodes.Status429TooManyRequests,
            problem.Status);

        Assert.Equal(
            "Savings plan usage limit exceeded.",
            problem.Title);

        Assert.Equal(
            $"The daily savings plan limit of {SubscriptionPlanLimits.PlusDailySavingsPlanLimit} requests has been reached.",
            problem.Detail);

        var hasLimit =
            problem.Extensions.TryGetValue(
                "limit",
                out var limitValue);

        Assert.True(hasLimit);

        var limit =
            Assert.IsType<JsonElement>(
                limitValue);

        Assert.Equal(
            SubscriptionPlanLimits
                .PlusDailySavingsPlanLimit,
            limit.GetInt32());
    }

    private static CreateSavingsPlanCommand CreateRequest()
    {
        return new CreateSavingsPlanCommand(
            SavingsPlanGoalType.MonthlyBudget,
            50m,
            [],
            SavingsPlanStrategy.Balanced,
            null,
            "en");
    }

    private static async Task SeedAsync(
        IServiceProvider services,
        Guid userId,
        SubscriptionPlan subscriptionPlan)
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
                UserName =
                    $"{userId}@example.com",
                Email =
                    $"{userId}@example.com",
                BaseCurrency =
                    Currency.PLN
            });

        if (subscriptionPlan != SubscriptionPlan.Free)
        {
            var periodStart =
                DateTimeOffset.UtcNow.AddDays(-1);

            var periodEnd =
                DateTimeOffset.UtcNow.AddMonths(1);

            dbContext.BillingSubscriptions.Add(
                new BillingSubscription(
                    Guid.NewGuid(),
                    userId,
                    subscriptionPlan,
                    BillingInterval.Monthly,
                    periodStart,
                    periodEnd));
        }

        dbContext.Subscriptions.AddRange(
            CreateSubscription(
                userId,
                "Netflix",
                60m,
                DigitalServiceCategory.Video),
            CreateSubscription(
                userId,
                "Spotify",
                40m,
                DigitalServiceCategory.Music));

        await dbContext.SaveChangesAsync();
    }

    private static async Task SeedUsageAsync(
        IServiceProvider services,
        Guid userId,
        int requestCount)
    {
        await using var scope =
            services.CreateAsyncScope();

        var dbContext =
            scope.ServiceProvider
                .GetRequiredService<
                    SubscriptionManagerDbContext>();

        var usage =
            new Domain.SavingsPlans.SavingsPlanUsage(
                userId,
                DateOnly.FromDateTime(
                    DateTime.UtcNow));

        for (var index = 0;
             index < requestCount;
             index++)
        {
            usage.RegisterRequest(
                requestCount);
        }

        dbContext.SavingsPlanUsages.Add(
            usage);

        await dbContext.SaveChangesAsync();
    }

    private static Subscription CreateSubscription(
        Guid ownerId,
        string name,
        decimal amount,
        DigitalServiceCategory category)
    {
        var subscription =
            new Subscription(
                Guid.NewGuid(),
                ownerId,
                name,
                amount,
                Currency.PLN,
                BillingPeriod.Monthly,
                new DateOnly(2026, 1, 1));

        subscription.AssignDigitalService(
            Guid.NewGuid(),
            category,
            null,
            null,
            null);

        return subscription;
    }

    private sealed class SuccessfulSavingsPlanAgent
        : ISavingsPlanAgent
    {
        public Task<SavingsPlanAgentResult> CreatePlanAsync(
            SavingsPlanAgentRequest request,
            CancellationToken cancellationToken = default)
        {
            var netflix =
                request.Subscriptions.Single(
                    subscription =>
                        subscription.Name ==
                        "Netflix");

            return Task.FromResult(
                new SavingsPlanAgentResult(
                    new SavingsPlanAgentScenario(
                        [netflix.Id],
                        "Ending Netflix reaches the selected budget."),
                    null));
        }
    }

    private sealed class UnavailableSavingsPlanAgent
        : ISavingsPlanAgent
    {
        public Task<SavingsPlanAgentResult> CreatePlanAsync(
            SavingsPlanAgentRequest request,
            CancellationToken cancellationToken = default)
        {
            return Task.FromException<
                SavingsPlanAgentResult>(
                new SavingsPlanUnavailableException(
                    "Technical provider details."));
        }
    }
}
