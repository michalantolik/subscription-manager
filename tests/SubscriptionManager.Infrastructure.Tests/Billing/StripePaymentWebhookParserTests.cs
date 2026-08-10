using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Options;
using SubscriptionManager.Application.Billing.ProcessWebhook;
using SubscriptionManager.Domain.Billing;
using SubscriptionManager.Infrastructure.Billing;

namespace SubscriptionManager.Infrastructure.Tests.Billing;

public sealed class StripePaymentWebhookParserTests
{
    private const string WebhookSecret =
        "whsec_test_secret";

    private const string SubscriptionPayload =
        """
        {
          "id": "evt_123",
          "object": "event",
          "api_version": "2026-07-29.dahlia",
          "created": 1786233600,
          "type": "customer.subscription.updated",
          "data": {
            "object": {
              "id": "sub_123",
              "object": "subscription",
              "customer": "cus_123",
              "status": "active",
              "cancel_at_period_end": false,
              "metadata": {
                "userId": "11111111-1111-1111-1111-111111111111"
              },
              "items": {
                "object": "list",
                "data": [
                  {
                    "id": "si_123",
                    "object": "subscription_item",
                    "current_period_start": 1786233600,
                    "current_period_end": 1788912000,
                    "price": {
                      "id": "price_plus_monthly",
                      "object": "price"
                    }
                  }
                ]
              }
            }
          }
        }
        """;

    [Fact]
    public void Parse_WithValidSubscriptionEvent_ShouldMapEvent()
    {
        var parser =
            CreateParser();

        var signature =
            CreateSignature(
                SubscriptionPayload);

        var result =
            parser.Parse(
                SubscriptionPayload,
                signature);

        Assert.NotNull(
            result);

        Assert.Equal(
            "evt_123",
            result.ProviderEventId);

        Assert.Equal(
            Guid.Parse(
                "11111111-1111-1111-1111-111111111111"),
            result.UserId);

        Assert.Equal(
            "cus_123",
            result.ProviderCustomerId);

        Assert.Equal(
            "sub_123",
            result.ProviderSubscriptionId);

        Assert.Equal(
            "price_plus_monthly",
            result.ProviderPriceId);

        Assert.Equal(
            SubscriptionPlan.Plus,
            result.Plan);

        Assert.Equal(
            BillingInterval.Monthly,
            result.BillingInterval);

        Assert.Equal(
            BillingSubscriptionStatus.Active,
            result.Status);

        Assert.False(
            result.CancelAtPeriodEnd);
    }

    [Fact]
    public void Parse_WithInvalidSignature_ShouldThrow()
    {
        var parser =
            CreateParser();

        Assert.Throws<InvalidPaymentWebhookException>(() =>
            parser.Parse(
                SubscriptionPayload,
                "t=1786233600,v1=invalid"));
    }

    [Fact]
    public void Parse_WithUnsupportedEvent_ShouldReturnNull()
    {
        const string payload =
            """
            {
              "id": "evt_ignored",
              "object": "event",
              "api_version": "2026-07-29.dahlia",
              "created": 1786233600,
              "type": "invoice.created",
              "data": {
                "object": {
                  "id": "in_123",
                  "object": "invoice"
                }
              }
            }
            """;

        var parser =
            CreateParser();

        var signature =
            CreateSignature(
                payload);

        var result =
            parser.Parse(
                payload,
                signature);

        Assert.Null(
            result);
    }

    private static StripePaymentWebhookParser CreateParser()
    {
        var options =
            Options.Create(
                new StripeOptions
                {
                    WebhookSecret = WebhookSecret,
                    PlusMonthlyPriceId =
                        "price_plus_monthly",
                    PlusYearlyPriceId =
                        "price_plus_yearly",
                    PremiumMonthlyPriceId =
                        "price_premium_monthly",
                    PremiumYearlyPriceId =
                        "price_premium_yearly"
                });

        return new StripePaymentWebhookParser(
            options);
    }

    private static string CreateSignature(
        string payload)
    {
        var timestamp =
            DateTimeOffset.UtcNow.ToUnixTimeSeconds();

        var signedPayload =
            $"{timestamp}.{payload}";

        using var hmac =
            new HMACSHA256(
                Encoding.UTF8.GetBytes(
                    WebhookSecret));

        var hash =
            hmac.ComputeHash(
                Encoding.UTF8.GetBytes(
                    signedPayload));

        return
            $"t={timestamp},v1={Convert.ToHexString(hash).ToLowerInvariant()}";
    }
}
