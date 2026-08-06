using SubscriptionManager.Blazor.Features.Authentication;
using SubscriptionManager.Blazor.Features.Currencies;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace SubscriptionManager.Blazor.Features.Subscriptions;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum BillingPeriod
{
    Monthly = 1,
    Quarterly = 2,
    SemiAnnual = 3,
    Yearly = 4
}

public sealed record SubscriptionResponse(
    Guid Id,
    Guid? DigitalServiceId,
    string Name,
    string? Category,
    string? CustomCategoryName,
    string? IconKey,
    string? ManagementUrl,
    decimal Amount,
    Currency Currency,
    BillingPeriod BillingPeriod,
    DateOnly StartDate,
    DateOnly? EndDate,
    bool IsActive,
    decimal MonthlyEquivalentAmount,
    decimal YearlyEquivalentAmount);

public sealed record SubscriptionCostSummaryResponse(
    Currency BaseCurrency,
    DateOnly? ExchangeRateEffectiveDate,
    int ActiveCount,
    int TotalCount,
    decimal MonthlyCost,
    decimal YearlyCost,
    decimal AverageMonthlyCost,
    decimal AverageYearlyCost,
    IReadOnlyList<SubscriptionCostSummaryItemResponse>
        TopSubscriptions,
    IReadOnlyList<SubscriptionCostSummaryItemResponse>
        ActiveSubscriptions,
    IReadOnlyList<SubscriptionCategoryCostSummaryResponse>
        Categories);

public sealed record SubscriptionCostSummaryItemResponse(
    Guid Id,
    string Name,
    BillingPeriod BillingPeriod,
    decimal MonthlyCost);

public sealed record SubscriptionCategoryCostSummaryResponse(
    string Category,
    string? CustomCategoryName,
    decimal MonthlyCost);

public sealed class SubscriptionFormModel
{
    public Guid? DigitalServiceId { get; set; }

    [Required]
    [StringLength(200)]
    public string Name { get; set; } = string.Empty;

    [PositiveDecimal]
    public decimal Amount { get; set; }

    public Currency Currency { get; set; } =
        Currency.PLN;

    public BillingPeriod BillingPeriod { get; set; } =
        BillingPeriod.Monthly;

    public DateOnly StartDate { get; set; } =
        DateOnly.FromDateTime(DateTime.Today);
}

public sealed class EndSubscriptionModel
{
    public DateOnly EndDate { get; set; } =
        DateOnly.FromDateTime(DateTime.Today);
}

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
public sealed class PositiveDecimalAttribute : ValidationAttribute
{
    public override bool IsValid(object? value)
    {
        return value is decimal amount && amount > 0;
    }
}

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
