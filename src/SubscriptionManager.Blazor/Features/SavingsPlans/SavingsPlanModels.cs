using SubscriptionManager.Blazor.Features.Authentication;
using SubscriptionManager.Blazor.Features.Currencies;
using System.Net;
using System.Security.Claims;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace SubscriptionManager.Blazor.Features.SavingsPlans;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum SavingsPlanGoalType
{
    MonthlyBudget = 1,
    MonthlySavings = 2
}

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum SavingsPlanStrategy
{
    FewerChanges = 1,
    Balanced = 2,
    MaximumSavings = 3
}

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum SubscriptionPlan
{
    Free = 1,
    Plus = 2,
    Premium = 3
}

public sealed record CreateSavingsPlanRequest(
    SavingsPlanGoalType GoalType,
    decimal TargetAmount,
    IReadOnlyCollection<Guid> ProtectedSubscriptionIds,
    SavingsPlanStrategy Strategy,
    string? AdditionalPreference,
    string LanguageCode);

public sealed record SavingsPlanResponse(
    Currency BaseCurrency,
    decimal CurrentMonthlyCost,
    SavingsPlanScenarioResponse? Recommended,
    SavingsPlanScenarioResponse? Alternative,
    SubscriptionPlan SubscriptionPlan,
    int DailyRequestLimit,
    int RemainingRequestCount);

public sealed record SavingsPlanScenarioResponse(
    IReadOnlyList<SavingsPlanSubscriptionResponse> Subscriptions,
    decimal ProjectedMonthlyCost,
    decimal MonthlySavings,
    decimal YearlySavings,
    bool TargetReached,
    string Explanation);

public sealed record SavingsPlanSubscriptionResponse(
    Guid Id,
    string Name,
    string Category,
    decimal MonthlyCost);

public sealed class SavingsPlanUsageLimitExceededException
    : Exception
{
    public SavingsPlanUsageLimitExceededException(
        string? message,
        int dailyLimit)
        : base(
            string.IsNullOrWhiteSpace(message)
                ? "The daily savings plan limit has been reached."
                : message)
    {
        DailyLimit = dailyLimit;
    }

    public int DailyLimit { get; }
}

public sealed class SavingsPlansApiClient(
    HttpClient httpClient)
{
    private static readonly JsonSerializerOptions JsonOptions =
        new(JsonSerializerDefaults.Web)
        {
            Converters =
            {
                new JsonStringEnumConverter()
            }
        };

    public async Task<SavingsPlanResponse> CreateAsync(
        CreateSavingsPlanRequest model,
        ClaimsPrincipal user,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(model);
        ArgumentNullException.ThrowIfNull(user);

        using var request =
            new HttpRequestMessage(
                HttpMethod.Post,
                "api/savings-plans")
            {
                Content = JsonContent.Create(
                    model,
                    options: JsonOptions)
            };

        ApiRequestAuthorization.AddBearerToken(
            request,
            user);

        using var response =
            await httpClient.SendAsync(
                request,
                cancellationToken);

        if (response.StatusCode ==
            HttpStatusCode.TooManyRequests)
        {
            var problemDetails =
                await response.Content
                    .ReadFromJsonAsync<ApiProblemDetails>(
                        JsonOptions,
                        cancellationToken);

            throw new SavingsPlanUsageLimitExceededException(
                problemDetails?.Detail,
                problemDetails?.DailyLimit ?? 0);
        }

        response.EnsureSuccessStatusCode();

        return await response.Content
                   .ReadFromJsonAsync<SavingsPlanResponse>(
                       JsonOptions,
                       cancellationToken)
               ?? throw new InvalidOperationException(
                   "The savings plan response was empty.");
    }

    private sealed record ApiProblemDetails(
        string? Detail,
        int DailyLimit);
}
