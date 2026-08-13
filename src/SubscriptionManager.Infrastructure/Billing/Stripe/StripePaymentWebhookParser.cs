using Microsoft.Extensions.Options;
using Stripe;
using SubscriptionManager.Application.Billing.ProcessWebhook;
using SubscriptionManager.Domain.Billing;

namespace SubscriptionManager.Infrastructure.Billing.Stripe;

/// <summary>
/// Parses Stripe webhook events into payment subscription events.
/// </summary>
public sealed class StripePaymentWebhookParser
    : IPaymentWebhookParser
{
    private const string UserIdMetadataKey =
        "userId";

    private readonly StripeOptions _options;

    private readonly StripePriceCatalog
        _priceCatalog;

    public StripePaymentWebhookParser(
        IOptions<StripeOptions> options)
    {
        _options = options.Value;

        _priceCatalog =
            new StripePriceCatalog(
                options);
    }

    public PaymentSubscriptionEvent? Parse(
        string payload,
        string signature)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(
            payload);

        ArgumentException.ThrowIfNullOrWhiteSpace(
            signature);

        if (string.IsNullOrWhiteSpace(
                _options.WebhookSecret))
        {
            throw new InvalidOperationException(
                "The Stripe webhook secret is not configured.");
        }

        Event stripeEvent;

        try
        {
            stripeEvent =
                EventUtility.ConstructEvent(
                    payload,
                    signature,
                    _options.WebhookSecret);
        }
        catch (StripeException exception)
        {
            throw new InvalidPaymentWebhookException(
                "The Stripe webhook signature is invalid.",
                exception);
        }

        if (!IsSupportedEvent(
                stripeEvent.Type))
        {
            return null;
        }

        if (stripeEvent.Data.Object is not
            Subscription subscription)
        {
            throw new InvalidPaymentWebhookException(
                "The Stripe event does not contain a subscription.");
        }

        var subscriptionItem =
            subscription.Items?
                .Data?
                .SingleOrDefault();

        if (subscriptionItem is null)
        {
            throw new InvalidPaymentWebhookException(
                "The Stripe subscription must contain exactly one item.");
        }

        var priceId =
            subscriptionItem.Price?.Id;

        if (string.IsNullOrWhiteSpace(
                priceId))
        {
            throw new InvalidPaymentWebhookException(
                "The Stripe subscription does not contain a price.");
        }

        var (
            plan,
            billingInterval) =
                MapPrice(
                    priceId);

        return new PaymentSubscriptionEvent(
            stripeEvent.Id,
            ToDateTimeOffset(
                stripeEvent.Created),
            GetUserId(
                subscription),
            GetRequiredValue(
                subscription.CustomerId,
                "The Stripe subscription does not contain a customer ID."),
            GetRequiredValue(
                subscription.Id,
                "The Stripe subscription does not contain an ID."),
            priceId,
            plan,
            billingInterval,
            MapStatus(
                subscription.Status),
            ToDateTimeOffset(
                subscriptionItem.CurrentPeriodStart),
            ToDateTimeOffset(
                subscriptionItem.CurrentPeriodEnd),
            subscription.CancelAtPeriodEnd);
    }

    private static bool IsSupportedEvent(
        string eventType)
    {
        return eventType is
            "customer.subscription.created" or
            "customer.subscription.updated" or
            "customer.subscription.deleted";
    }

    private (
        SubscriptionPlan Plan,
        BillingInterval BillingInterval) MapPrice(
            string priceId)
    {
        if (_priceCatalog.TryGetPlan(
                priceId,
                out var plan,
                out var billingInterval))
        {
            return (
                plan,
                billingInterval);
        }

        throw new InvalidPaymentWebhookException(
            $"The Stripe price '{priceId}' is not configured.");
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
                throw new InvalidPaymentWebhookException(
                    $"The Stripe subscription status '{status}' is not supported.")
        };
    }

    private static Guid? GetUserId(
        Subscription subscription)
    {
        if (subscription.Metadata is null ||
            !subscription.Metadata.TryGetValue(
                UserIdMetadataKey,
                out var value))
        {
            return null;
        }

        if (!Guid.TryParse(
                value,
                out var userId))
        {
            throw new InvalidPaymentWebhookException(
                "The Stripe subscription contains an invalid user ID.");
        }

        return userId;
    }

    private static string GetRequiredValue(
        string? value,
        string errorMessage)
    {
        if (string.IsNullOrWhiteSpace(
                value))
        {
            throw new InvalidPaymentWebhookException(
                errorMessage);
        }

        return value;
    }

    private static DateTimeOffset ToDateTimeOffset(
        DateTime value)
    {
        return new DateTimeOffset(
            DateTime.SpecifyKind(
                value,
                DateTimeKind.Utc));
    }
}
