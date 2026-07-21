using System.ComponentModel.DataAnnotations;
using System.Net.Http.Json;
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
    string Name,
    decimal Amount,
    string Currency,
    BillingPeriod BillingPeriod,
    DateOnly StartDate,
    DateOnly? EndDate,
    bool IsActive,
    decimal MonthlyEquivalentAmount,
    decimal YearlyEquivalentAmount);

public sealed class SubscriptionFormModel
{
    [Required]
    [StringLength(200)]
    public string Name { get; set; } = string.Empty;

    [PositiveDecimal]
    public decimal Amount { get; set; }

    [Required]
    [StringLength(3, MinimumLength = 3)]
    public string Currency { get; set; } = "PLN";

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

public sealed class SubscriptionsApiClient(HttpClient httpClient)
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
        CancellationToken cancellationToken = default)
    {
        return await httpClient.GetFromJsonAsync<List<SubscriptionResponse>>(
                   "api/subscriptions",
                   JsonOptions,
                   cancellationToken)
               ?? [];
    }

    public Task<SubscriptionResponse?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        return httpClient.GetFromJsonAsync<SubscriptionResponse>(
            $"api/subscriptions/{id}",
            JsonOptions,
            cancellationToken);
    }

    public async Task<Guid> CreateAsync(
        SubscriptionFormModel model,
        CancellationToken cancellationToken = default)
    {
        using var response = await httpClient.PostAsJsonAsync(
            "api/subscriptions",
            new
            {
                model.Name,
                model.Amount,
                Currency = model.Currency.ToUpperInvariant(),
                model.BillingPeriod,
                model.StartDate
            },
            JsonOptions,
            cancellationToken);

        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<Guid>(
            JsonOptions,
            cancellationToken);
    }

    public async Task UpdateAsync(
        Guid id,
        SubscriptionFormModel model,
        CancellationToken cancellationToken = default)
    {
        using var response = await httpClient.PutAsJsonAsync(
            $"api/subscriptions/{id}",
            new
            {
                model.Name,
                model.Amount,
                Currency = model.Currency.ToUpperInvariant(),
                model.BillingPeriod
            },
            JsonOptions,
            cancellationToken);

        response.EnsureSuccessStatusCode();
    }

    public async Task EndAsync(
        Guid id,
        DateOnly endDate,
        CancellationToken cancellationToken = default)
    {
        using var response = await httpClient.PostAsJsonAsync(
            $"api/subscriptions/{id}/end",
            new
            {
                EndDate = endDate
            },
            JsonOptions,
            cancellationToken);

        response.EnsureSuccessStatusCode();
    }

    public async Task DeleteAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        using var response = await httpClient.DeleteAsync(
            $"api/subscriptions/{id}",
            cancellationToken);

        response.EnsureSuccessStatusCode();
    }
}
