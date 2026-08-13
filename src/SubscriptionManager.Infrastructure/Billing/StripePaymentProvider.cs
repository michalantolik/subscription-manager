using Microsoft.Extensions.Options;
using Stripe;
using Stripe.Checkout;
using SubscriptionManager.Application.Billing.PaymentProvider;
using SubscriptionManager.Domain.Billing;

namespace SubscriptionManager.Infrastructure.Billing;

public sealed class StripePaymentProvider
    : IPaymentProvider
{
    private const string UserIdMetadataKey =
        "userId";

    private readonly StripeOptions _options;

    private readonly StripePriceCatalog
        _priceCatalog;

    public StripePaymentProvider(
        IOptions<StripeOptions> options)
    {
        _options = options.Value;

        _priceCatalog =
            new StripePriceCatalog(
                options);
    }

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
            _priceCatalog.GetPriceId(
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
                cancellationToken:
                    cancellationToken);

        return new Uri(
            session.Url);
    }
}
