using System.Net;
using System.Security.Claims;
using System.Text.Json;
using System.Text.Json.Serialization;
using SubscriptionManager.Web.Features.Authentication.Security;

namespace SubscriptionManager.Web.Features.SavingsPlans;

/// <summary>
/// Provides access to savings plan-related API operations.
/// </summary>
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

    public async Task<SavingsPlanUsageResponse> GetUsageAsync(
        ClaimsPrincipal user,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(user);

        using var request =
            new HttpRequestMessage(
                HttpMethod.Get,
                "api/savings-plans/usage");

        ApiRequestAuthorization.AddBearerToken(
            request,
            user);

        using var response =
            await httpClient.SendAsync(
                request,
                cancellationToken);

        response.EnsureSuccessStatusCode();

        return await response.Content
                   .ReadFromJsonAsync<SavingsPlanUsageResponse>(
                       JsonOptions,
                       cancellationToken)
               ?? throw new InvalidOperationException(
                   "The savings plan usage response was empty.");
    }

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
