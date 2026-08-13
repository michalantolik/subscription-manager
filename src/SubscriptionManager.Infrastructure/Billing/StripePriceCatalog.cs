using Microsoft.Extensions.Options;
using Stripe;
using SubscriptionManager.Application.Billing.PaymentProvider;
using SubscriptionManager.Domain.Billing;

namespace SubscriptionManager.Infrastructure.Billing;

public sealed class StripePriceCatalog(
    IOptions<StripeOptions> options,
    TimeProvider? timeProvider = null)
    : IPaymentPlanCatalog
{
    private static readonly TimeSpan CacheDuration =
        TimeSpan.FromMinutes(15);

    private readonly StripeOptions _options =
        options.Value;

    private readonly TimeProvider _timeProvider =
        timeProvider ??
        TimeProvider.System;

    private readonly SemaphoreSlim _cacheLock =
        new(
            initialCount: 1,
            maxCount: 1);

    private IReadOnlyList<PaymentPlanPrice>? _cachedPrices;
    private DateTimeOffset _cacheExpiresAt;

    public async Task<IReadOnlyList<PaymentPlanPrice>>
        GetPricesAsync(
            CancellationToken cancellationToken = default)
    {
        var now =
            _timeProvider.GetUtcNow();

        if (_cachedPrices is not null &&
            now < _cacheExpiresAt)
        {
            return _cachedPrices;
        }

        await _cacheLock.WaitAsync(
            cancellationToken);

        try
        {
            now =
                _timeProvider.GetUtcNow();

            if (_cachedPrices is not null &&
                now < _cacheExpiresAt)
            {
                return _cachedPrices;
            }

            if (string.IsNullOrWhiteSpace(
                    _options.SecretKey))
            {
                throw new InvalidOperationException(
                    "The Stripe secret key is not configured.");
            }

            var client =
                new StripeClient(
                    _options.SecretKey);

            var priceService =
                new PriceService(
                    client);

            var configuredPrices =
                GetConfiguredPrices();

            var priceTasks =
                configuredPrices.Select(
                    async configuredPrice =>
                    {
                        var stripePrice =
                            await priceService.GetAsync(
                                configuredPrice.PriceId,
                                cancellationToken:
                                    cancellationToken);

                        ValidateStripePrice(
                            stripePrice,
                            configuredPrice.BillingInterval);

                        return new PaymentPlanPrice(
                            configuredPrice.Plan,
                            configuredPrice.BillingInterval,
                            ConvertFromMinorUnits(
                                stripePrice.UnitAmount!.Value),
                            stripePrice.Currency
                                .ToUpperInvariant());
                    });

            var prices =
                await Task.WhenAll(
                    priceTasks);

            _cachedPrices =
                prices;

            _cacheExpiresAt =
                now.Add(
                    CacheDuration);

            return _cachedPrices;
        }
        finally
        {
            _cacheLock.Release();
        }
    }

    public string GetPriceId(
        SubscriptionPlan plan,
        BillingInterval billingInterval)
    {
        return (plan, billingInterval) switch
        {
            (
                SubscriptionPlan.Plus,
                BillingInterval.Monthly) =>
                    _options.PlusMonthlyPriceId,

            (
                SubscriptionPlan.Plus,
                BillingInterval.Yearly) =>
                    _options.PlusYearlyPriceId,

            (
                SubscriptionPlan.Premium,
                BillingInterval.Monthly) =>
                    _options.PremiumMonthlyPriceId,

            (
                SubscriptionPlan.Premium,
                BillingInterval.Yearly) =>
                    _options.PremiumYearlyPriceId,

            _ =>
                throw new ArgumentOutOfRangeException(
                    nameof(plan),
                    plan,
                    "The selected subscription plan and billing interval are not supported.")
        };
    }

    public bool TryGetPlan(
        string priceId,
        out SubscriptionPlan plan,
        out BillingInterval billingInterval)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(
            priceId);

        if (priceId ==
            _options.PlusMonthlyPriceId)
        {
            plan = SubscriptionPlan.Plus;
            billingInterval =
                BillingInterval.Monthly;

            return true;
        }

        if (priceId ==
            _options.PlusYearlyPriceId)
        {
            plan = SubscriptionPlan.Plus;
            billingInterval =
                BillingInterval.Yearly;

            return true;
        }

        if (priceId ==
            _options.PremiumMonthlyPriceId)
        {
            plan = SubscriptionPlan.Premium;
            billingInterval =
                BillingInterval.Monthly;

            return true;
        }

        if (priceId ==
            _options.PremiumYearlyPriceId)
        {
            plan = SubscriptionPlan.Premium;
            billingInterval =
                BillingInterval.Yearly;

            return true;
        }

        plan = default;
        billingInterval = default;

        return false;
    }

    private IReadOnlyList<ConfiguredPrice>
        GetConfiguredPrices()
    {
        return
        [
            new ConfiguredPrice(
                SubscriptionPlan.Plus,
                BillingInterval.Monthly,
                _options.PlusMonthlyPriceId),

            new ConfiguredPrice(
                SubscriptionPlan.Plus,
                BillingInterval.Yearly,
                _options.PlusYearlyPriceId),

            new ConfiguredPrice(
                SubscriptionPlan.Premium,
                BillingInterval.Monthly,
                _options.PremiumMonthlyPriceId),

            new ConfiguredPrice(
                SubscriptionPlan.Premium,
                BillingInterval.Yearly,
                _options.PremiumYearlyPriceId)
        ];
    }

    private static void ValidateStripePrice(
        Price price,
        BillingInterval billingInterval)
    {
        if (!price.Active)
        {
            throw new InvalidOperationException(
                $"The configured Stripe price '{price.Id}' is inactive.");
        }

        if (price.UnitAmount is null)
        {
            throw new InvalidOperationException(
                $"The configured Stripe price '{price.Id}' does not have a unit amount.");
        }

        if (string.IsNullOrWhiteSpace(
                price.Currency))
        {
            throw new InvalidOperationException(
                $"The configured Stripe price '{price.Id}' does not have a currency.");
        }

        var expectedInterval =
            billingInterval switch
            {
                BillingInterval.Monthly =>
                    "month",

                BillingInterval.Yearly =>
                    "year",

                _ =>
                    throw new ArgumentOutOfRangeException(
                        nameof(billingInterval),
                        billingInterval,
                        "The billing interval is not supported.")
            };

        if (price.Recurring is null ||
            price.Recurring.Interval !=
            expectedInterval)
        {
            throw new InvalidOperationException(
                $"The configured Stripe price '{price.Id}' does not match the expected billing interval.");
        }
    }

    private static decimal ConvertFromMinorUnits(
        long amount)
    {
        return amount / 100m;
    }

    private sealed record ConfiguredPrice(
        SubscriptionPlan Plan,
        BillingInterval BillingInterval,
        string PriceId);
}
