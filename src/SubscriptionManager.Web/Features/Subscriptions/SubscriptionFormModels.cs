using System.ComponentModel.DataAnnotations;
using SubscriptionManager.Web.Common.Currencies;

namespace SubscriptionManager.Web.Features.Subscriptions;

/// <summary>
/// Form data for creating or updating a subscription.
/// </summary>
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

/// <summary>
/// Form data for ending a subscription.
/// </summary>
public sealed class EndSubscriptionModel
{
    public DateOnly EndDate { get; set; } =
        DateOnly.FromDateTime(DateTime.Today);
}

/// <summary>
/// Validates that a decimal value is greater than zero.
/// </summary>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
public sealed class PositiveDecimalAttribute : ValidationAttribute
{
    public override bool IsValid(object? value)
    {
        return value is decimal amount && amount > 0;
    }
}
