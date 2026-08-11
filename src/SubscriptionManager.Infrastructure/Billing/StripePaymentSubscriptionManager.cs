using Microsoft.Extensions.Options;
using Stripe;
using SubscriptionManager.Application.Billing;
using SubscriptionManager.Domain.Billing;

namespace SubscriptionManager.Infrastructure.Billing;

public sealed class StripePaymentSubscriptionManager(
    IOptions<StripeOptions> options)
    : IPaymentSubscriptionManager
{
    private const string AlwaysInvoiceProrationBehavior =
        "always_invoice";

    private const string NoProrationBehavior =
        "none";

    private const string PendingIfIncompletePaymentBehavior =
        "pending_if_incomplete";

    private const string ReleaseScheduleEndBehavior =
        "release";

    private readonly StripeOptions _options =
        options.Value;

    private readonly StripePriceCatalog _priceCatalog =
        new(options);

    public async Task<PaymentSubscriptionChangePreview>
        PreviewChangeAsync(
            string providerSubscriptionId,
            SubscriptionPlan targetPlan,
            BillingInterval targetBillingInterval,
            BillingSubscriptionChangeTiming timing,
            CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(
            providerSubscriptionId);

        var client =
            CreateClient();

        var subscription =
            await GetSubscriptionAsync(
                client,
                providerSubscriptionId,
                cancellationToken);

        var subscriptionItem =
            GetSubscriptionItem(
                subscription);

        if (timing ==
            BillingSubscriptionChangeTiming.NextBillingPeriod)
        {
            return new PaymentSubscriptionChangePreview(
                AmountDueNow: 0m,
                Currency:
                    subscription.Currency.ToUpperInvariant(),
                EffectiveAt:
                    ToDateTimeOffset(
                        subscriptionItem.CurrentPeriodEnd));
        }

        var targetPriceId =
            _priceCatalog.GetPriceId(
                targetPlan,
                targetBillingInterval);

        var prorationDate =
            DateTime.UtcNow;

        var invoiceService =
            new InvoiceService(
                client);

        var invoice =
            await invoiceService.CreatePreviewAsync(
                new InvoiceCreatePreviewOptions
                {
                    Subscription =
                        providerSubscriptionId,
                    SubscriptionDetails =
                        new InvoiceSubscriptionDetailsOptions
                        {
                            ProrationBehavior =
                                AlwaysInvoiceProrationBehavior,
                            ProrationDate =
                                prorationDate,
                            Items =
                            [
                                new InvoiceSubscriptionDetailsItemOptions
                                {
                                    Id =
                                        subscriptionItem.Id,
                                    Price =
                                        targetPriceId,
                                    Quantity =
                                        subscriptionItem.Quantity
                                }
                            ]
                        }
                },
                cancellationToken:
                    cancellationToken);

        return new PaymentSubscriptionChangePreview(
            AmountDueNow:
                ConvertFromMinorUnits(
                    invoice.AmountDue),
            Currency:
                invoice.Currency.ToUpperInvariant(),
            EffectiveAt:
                ToDateTimeOffset(
                    prorationDate));
    }

    public async Task<PaymentSubscriptionChangeResult> ChangeAsync(
        string providerSubscriptionId,
        SubscriptionPlan targetPlan,
        BillingInterval targetBillingInterval,
        BillingSubscriptionChangeTiming timing,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(
            providerSubscriptionId);

        var client =
            CreateClient();

        var subscription =
            await GetSubscriptionAsync(
                client,
                providerSubscriptionId,
                cancellationToken);

        var subscriptionItem =
            GetSubscriptionItem(
                subscription);

        var targetPriceId =
            _priceCatalog.GetPriceId(
                targetPlan,
                targetBillingInterval);

        if (timing ==
            BillingSubscriptionChangeTiming.Immediate)
        {
            var updatedSubscription =
                await ChangeImmediatelyAsync(
                    client,
                    subscription,
                    subscriptionItem,
                    targetPriceId,
                    cancellationToken);

            return new PaymentSubscriptionChangeResult(
                ToPaymentSubscriptionState(
                    updatedSubscription));
        }

        await ScheduleChangeAsync(
            client,
            subscription,
            subscriptionItem,
            targetPriceId,
            targetBillingInterval,
            cancellationToken);

        return new PaymentSubscriptionChangeResult(
            UpdatedSubscription: null);
    }

    public async Task<PaymentSubscriptionState> ScheduleCancellationAsync(
        string providerSubscriptionId,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(
            providerSubscriptionId);

        var subscriptionService =
            new SubscriptionService(
                CreateClient());

        var updatedSubscription =
            await subscriptionService.UpdateAsync(
                providerSubscriptionId,
                new SubscriptionUpdateOptions
                {
                    CancelAtPeriodEnd = true
                },
                cancellationToken:
                    cancellationToken);

        return ToPaymentSubscriptionState(
            updatedSubscription);
    }

    public async Task<PaymentSubscriptionState> ResumeAsync(
        string providerSubscriptionId,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(
            providerSubscriptionId);

        var subscriptionService =
            new SubscriptionService(
                CreateClient());

        var updatedSubscription =
            await subscriptionService.UpdateAsync(
                providerSubscriptionId,
                new SubscriptionUpdateOptions
                {
                    CancelAtPeriodEnd = false
                },
                cancellationToken:
                    cancellationToken);

        return ToPaymentSubscriptionState(
            updatedSubscription);
    }

    private static async Task<Subscription> ChangeImmediatelyAsync(
        StripeClient client,
        Subscription subscription,
        SubscriptionItem subscriptionItem,
        string targetPriceId,
        CancellationToken cancellationToken)
    {
        var subscriptionService =
            new SubscriptionService(
                client);

        return await subscriptionService.UpdateAsync(
            subscription.Id,
            new SubscriptionUpdateOptions
            {
                PaymentBehavior =
                    PendingIfIncompletePaymentBehavior,
                ProrationBehavior =
                    AlwaysInvoiceProrationBehavior,
                Items =
                [
                    new SubscriptionItemOptions
                    {
                        Id =
                            subscriptionItem.Id,
                        Price =
                            targetPriceId,
                        Quantity =
                            subscriptionItem.Quantity
                    }
                ]
            },
            cancellationToken:
                cancellationToken);
    }

    private static async Task ScheduleChangeAsync(
        StripeClient client,
        Subscription subscription,
        SubscriptionItem subscriptionItem,
        string targetPriceId,
        BillingInterval targetBillingInterval,
        CancellationToken cancellationToken)
    {
        var scheduleService =
            new SubscriptionScheduleService(
                client);

        var schedule =
            await scheduleService.CreateAsync(
                new SubscriptionScheduleCreateOptions
                {
                    FromSubscription =
                        subscription.Id
                },
                cancellationToken:
                    cancellationToken);

        await scheduleService.UpdateAsync(
            schedule.Id,
            new SubscriptionScheduleUpdateOptions
            {
                EndBehavior =
                    ReleaseScheduleEndBehavior,
                ProrationBehavior =
                    NoProrationBehavior,
                Phases =
                [
                    new SubscriptionSchedulePhaseOptions
                    {
                        StartDate =
                            subscriptionItem.CurrentPeriodStart,
                        EndDate =
                            subscriptionItem.CurrentPeriodEnd,
                        ProrationBehavior =
                            NoProrationBehavior,
                        Items =
                        [
                            new SubscriptionSchedulePhaseItemOptions
                            {
                                Price =
                                    subscriptionItem.Price.Id,
                                Quantity =
                                    subscriptionItem.Quantity
                            }
                        ]
                    },
                    new SubscriptionSchedulePhaseOptions
                    {
                        StartDate =
                            subscriptionItem.CurrentPeriodEnd,
                        Duration =
                            new SubscriptionSchedulePhaseDurationOptions
                            {
                                Interval =
                                    GetStripeInterval(
                                        targetBillingInterval),
                                IntervalCount = 1
                            },
                        ProrationBehavior =
                            NoProrationBehavior,
                        Items =
                        [
                            new SubscriptionSchedulePhaseItemOptions
                            {
                                Price =
                                    targetPriceId,
                                Quantity =
                                    subscriptionItem.Quantity
                            }
                        ]
                    }
                ]
            },
            cancellationToken:
                cancellationToken);
    }

    private static async Task<Subscription>
        GetSubscriptionAsync(
            StripeClient client,
            string providerSubscriptionId,
            CancellationToken cancellationToken)
    {
        var subscriptionService =
            new SubscriptionService(
                client);

        return await subscriptionService.GetAsync(
            providerSubscriptionId,
            cancellationToken:
                cancellationToken);
    }

    private static SubscriptionItem GetSubscriptionItem(
        Subscription subscription)
    {
        var items =
            subscription.Items.Data;

        if (items.Count != 1)
        {
            throw new InvalidOperationException(
                "The Stripe subscription must contain exactly one subscription item.");
        }

        return items[0];
    }

    private StripeClient CreateClient()
    {
        if (string.IsNullOrWhiteSpace(
                _options.SecretKey))
        {
            throw new InvalidOperationException(
                "The Stripe secret key is not configured.");
        }

        return new StripeClient(
            _options.SecretKey);
    }

    private PaymentSubscriptionState ToPaymentSubscriptionState(
        Subscription subscription)
    {
        var subscriptionItem =
            GetSubscriptionItem(
                subscription);

        var priceId =
            subscriptionItem.Price?.Id;

        if (string.IsNullOrWhiteSpace(
                priceId))
        {
            throw new InvalidOperationException(
                "The Stripe subscription does not contain a price.");
        }

        if (!_priceCatalog.TryGetPlan(
                priceId,
                out var plan,
                out var billingInterval))
        {
            throw new InvalidOperationException(
                $"The Stripe price '{priceId}' is not configured.");
        }

        return new PaymentSubscriptionState(
            plan,
            billingInterval,
            MapStatus(
                subscription.Status),
            priceId,
            ToDateTimeOffset(
                subscriptionItem.CurrentPeriodStart),
            ToDateTimeOffset(
                subscriptionItem.CurrentPeriodEnd),
            subscription.CancelAtPeriodEnd);
    }

    private static BillingSubscriptionStatus MapStatus(
        string status)
    {
        return status switch
        {
            "incomplete" =>
                BillingSubscriptionStatus.Incomplete,

            "incomplete_expired" =>
                BillingSubscriptionStatus.IncompleteExpired,

            "trialing" =>
                BillingSubscriptionStatus.Trialing,

            "active" =>
                BillingSubscriptionStatus.Active,

            "past_due" =>
                BillingSubscriptionStatus.PastDue,

            "canceled" =>
                BillingSubscriptionStatus.Canceled,

            "unpaid" =>
                BillingSubscriptionStatus.Unpaid,

            "paused" =>
                BillingSubscriptionStatus.Paused,

            _ =>
                throw new InvalidOperationException(
                    $"The Stripe subscription status '{status}' is not supported.")
        };
    }

    private static string GetStripeInterval(
        BillingInterval billingInterval)
    {
        return billingInterval switch
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
    }

    private static decimal ConvertFromMinorUnits(
        long amount)
    {
        return amount / 100m;
    }

    private static DateTimeOffset ToDateTimeOffset(
        DateTime date)
    {
        return new DateTimeOffset(
            DateTime.SpecifyKind(
                date,
                DateTimeKind.Utc));
    }
}
