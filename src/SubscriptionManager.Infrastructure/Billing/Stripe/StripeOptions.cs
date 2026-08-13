namespace SubscriptionManager.Infrastructure.Billing.Stripe;

/// <summary>
/// Configuration options for Stripe billing.
/// </summary>
public sealed class StripeOptions
{
    public const string SectionName = "Stripe";

    public string SecretKey { get; init; } = string.Empty;

    public string WebhookSecret { get; init; } = string.Empty;

    public string PlusMonthlyPriceId { get; init; } = string.Empty;

    public string PlusYearlyPriceId { get; init; } = string.Empty;

    public string PremiumMonthlyPriceId { get; init; } = string.Empty;

    public string PremiumYearlyPriceId { get; init; } = string.Empty;
}
