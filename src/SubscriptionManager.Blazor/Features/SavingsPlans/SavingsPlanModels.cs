using SubscriptionManager.Blazor.Features.Authentication;
using SubscriptionManager.Blazor.Features.Currencies;
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
    SavingsPlanScenarioResponse? Alternative);

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

        response.EnsureSuccessStatusCode();

        return await response.Content
                   .ReadFromJsonAsync<SavingsPlanResponse>(
                       JsonOptions,
                       cancellationToken)
               ?? throw new InvalidOperationException(
                   "The savings plan response was empty.");
    }
}
