using System.Security.Claims;
using System.Text.Json;
using System.Text.Json.Serialization;
using SubscriptionManager.Web.Features.Authentication.Security;

namespace SubscriptionManager.Web.Features.Subscriptions;

/// <summary>
/// Provides access to subscription-related API operations.
/// </summary>
public sealed class SubscriptionsApiClient(
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

    public async Task<IReadOnlyList<SubscriptionResponse>> GetAllAsync(
        ClaimsPrincipal user,
        CancellationToken cancellationToken = default)
    {
        using var request = new HttpRequestMessage(
            HttpMethod.Get,
            "api/subscriptions");

        ApiRequestAuthorization.AddBearerToken(
            request,
            user);

        using var response = await httpClient.SendAsync(
            request,
            cancellationToken);

        response.EnsureSuccessStatusCode();

        return await response.Content
                   .ReadFromJsonAsync<List<SubscriptionResponse>>(
                       JsonOptions,
                       cancellationToken)
               ?? [];
    }

    public async Task<SubscriptionCostSummaryResponse>
        GetCostSummaryAsync(
            ClaimsPrincipal user,
            CancellationToken cancellationToken = default)
    {
        using var request = new HttpRequestMessage(
            HttpMethod.Get,
            "api/subscriptions/cost-summary");

        ApiRequestAuthorization.AddBearerToken(
            request,
            user);

        using var response = await httpClient.SendAsync(
            request,
            cancellationToken);

        response.EnsureSuccessStatusCode();

        return await response.Content
                   .ReadFromJsonAsync<
                       SubscriptionCostSummaryResponse>(
                       JsonOptions,
                       cancellationToken)
               ?? throw new InvalidOperationException(
                   "The subscription cost summary response was empty.");
    }

    public async Task<SubscriptionResponse?> GetByIdAsync(
        Guid id,
        ClaimsPrincipal user,
        CancellationToken cancellationToken = default)
    {
        using var request = new HttpRequestMessage(
            HttpMethod.Get,
            $"api/subscriptions/{id}");

        ApiRequestAuthorization.AddBearerToken(
            request,
            user);

        using var response = await httpClient.SendAsync(
            request,
            cancellationToken);

        response.EnsureSuccessStatusCode();

        return await response.Content
            .ReadFromJsonAsync<SubscriptionResponse>(
                JsonOptions,
                cancellationToken);
    }

    public async Task<Guid> CreateAsync(
        SubscriptionFormModel model,
        ClaimsPrincipal user,
        CancellationToken cancellationToken = default)
    {
        using var request = new HttpRequestMessage(
            HttpMethod.Post,
            "api/subscriptions")
        {
            Content = JsonContent.Create(
                new
                {
                    model.Name,
                    model.Amount,
                    model.Currency,
                    model.BillingPeriod,
                    model.StartDate,
                    model.DigitalServiceId
                },
                options: JsonOptions)
        };

        ApiRequestAuthorization.AddBearerToken(
            request,
            user);

        using var response = await httpClient.SendAsync(
            request,
            cancellationToken);

        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<Guid>(
            JsonOptions,
            cancellationToken);
    }

    public async Task UpdateAsync(
        Guid id,
        SubscriptionFormModel model,
        ClaimsPrincipal user,
        CancellationToken cancellationToken = default)
    {
        using var request = new HttpRequestMessage(
            HttpMethod.Put,
            $"api/subscriptions/{id}")
        {
            Content = JsonContent.Create(
                new
                {
                    model.Name,
                    model.Amount,
                    model.Currency,
                    model.BillingPeriod,
                    model.DigitalServiceId
                },
                options: JsonOptions)
        };

        ApiRequestAuthorization.AddBearerToken(
            request,
            user);

        using var response = await httpClient.SendAsync(
            request,
            cancellationToken);

        response.EnsureSuccessStatusCode();
    }

    public async Task EndAsync(
        Guid id,
        DateOnly endDate,
        ClaimsPrincipal user,
        CancellationToken cancellationToken = default)
    {
        using var request = new HttpRequestMessage(
            HttpMethod.Post,
            $"api/subscriptions/{id}/end")
        {
            Content = JsonContent.Create(
                new
                {
                    EndDate = endDate
                },
                options: JsonOptions)
        };

        ApiRequestAuthorization.AddBearerToken(
            request,
            user);

        using var response = await httpClient.SendAsync(
            request,
            cancellationToken);

        response.EnsureSuccessStatusCode();
    }

    public async Task DeleteAsync(
        Guid id,
        ClaimsPrincipal user,
        CancellationToken cancellationToken = default)
    {
        using var request = new HttpRequestMessage(
            HttpMethod.Delete,
            $"api/subscriptions/{id}");

        ApiRequestAuthorization.AddBearerToken(
            request,
            user);

        using var response = await httpClient.SendAsync(
            request,
            cancellationToken);

        response.EnsureSuccessStatusCode();
    }
}
