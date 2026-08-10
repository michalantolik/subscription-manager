using Microsoft.Extensions.Options;
using Stripe;
using Stripe.Checkout;
using SubscriptionManager.Application.Billing;
using SubscriptionManager.Domain.Billing;

namespace SubscriptionManager.Infrastructure.Billing;

public sealed class StripePaymentProvider(
    IOptions<StripeOptions> options)
    : IPaymentProvider
{
    private const string UserIdMetadataKey = "userId";

    private readonly StripeOptions _options = options.Value;

    public async Task<Uri> CreateCheckoutSessionAsync(
        Guid userId,
        string email,
        SubscriptionPlan plan,
        BillingInterval billingInterval,
        Uri successUrl,
        Uri cancelUrl,
        CancellationToken cancellationToken = default)
    {
        var priceId =
            GetPriceId(
                plan,
                billingInterval);

        var client =
            new StripeClient(
                _options.SecretKey);

        var sessionOptions =
            new SessionCreateOptions
            {
                Mode = "subscription",
                CustomerEmail = email,
                SuccessUrl = successUrl.ToString(),
                CancelUrl = cancelUrl.ToString(),
                ClientReferenceId = userId.ToString(),
                SubscriptionData =
                    new SessionSubscriptionDataOptions
                    {
                        Metadata =
                            new Dictionary<string, string>
                            {
                                [UserIdMetadataKey] =
                                    userId.ToString()
                            }
                    },
                LineItems =
                [
                    new SessionLineItemOptions
                    {
                        Price = priceId,
                        Quantity = 1
                    }
                ]
            };

        var session =
            await client.V1.Checkout.Sessions.CreateAsync(
                sessionOptions,
                cancellationToken: cancellationToken);

        return new Uri(
            session.Url);
    }

    private string GetPriceId(
        SubscriptionPlan plan,
        BillingInterval billingInterval)
    {
        return (plan, billingInterval) switch
        {
            (SubscriptionPlan.Plus, BillingInterval.Monthly) =>
                _options.PlusMonthlyPriceId,

            (SubscriptionPlan.Plus, BillingInterval.Yearly) =>
                _options.PlusYearlyPriceId,

            (SubscriptionPlan.Premium, BillingInterval.Monthly) =>
                _options.PremiumMonthlyPriceId,

            (SubscriptionPlan.Premium, BillingInterval.Yearly) =>
                _options.PremiumYearlyPriceId,

            _ =>
                throw new ArgumentOutOfRangeException(
                    nameof(plan),
                    "The selected subscription plan and billing interval are not supported.")
        };
    }
}
