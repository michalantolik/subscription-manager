using System.Net;
using System.Text;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using SubscriptionManager.Application.Billing.ProcessWebhook;

namespace SubscriptionManager.Api.Tests.Billing;

public sealed class ProcessPaymentWebhookTests
    : IClassFixture<CustomWebApplicationFactory>
{
    private const string WebhookUrl =
        "/api/billing/webhook";

    private readonly CustomWebApplicationFactory _factory;

    public ProcessPaymentWebhookTests(
        CustomWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task PostAsync_ShouldAcceptWebhookWithoutAuthentication()
    {
        const string payload =
            """{"id":"evt_123"}""";

        const string signature =
            "t=123,v1=signature";

        var parser =
            new RecordingPaymentWebhookParser();

        using var factory =
            CreateFactory(
                parser);

        using var client =
            factory.CreateClient();

        using var request =
            CreateRequest(
                payload,
                signature);

        var response =
            await client.SendAsync(
                request);

        Assert.Equal(
            HttpStatusCode.NoContent,
            response.StatusCode);

        Assert.Equal(
            payload,
            parser.Payload);

        Assert.Equal(
            signature,
            parser.Signature);
    }

    [Fact]
    public async Task PostAsync_WithInvalidWebhook_ShouldReturnBadRequest()
    {
        using var factory =
            CreateFactory(
                new InvalidPaymentWebhookParser());

        using var client =
            factory.CreateClient();

        using var request =
            CreateRequest(
                """{"id":"evt_invalid"}""",
                "invalid-signature");

        var response =
            await client.SendAsync(
                request);

        Assert.Equal(
            HttpStatusCode.BadRequest,
            response.StatusCode);
    }

    private WebApplicationFactory<Program> CreateFactory(
        IPaymentWebhookParser parser)
    {
        return _factory.WithWebHostBuilder(
            builder =>
            {
                builder.ConfigureTestServices(
                    services =>
                    {
                        services.RemoveAll<
                            IPaymentWebhookParser>();

                        services.AddSingleton(
                            parser);
                    });
            });
    }

    private static HttpRequestMessage CreateRequest(
        string payload,
        string signature)
    {
        var request =
            new HttpRequestMessage(
                HttpMethod.Post,
                WebhookUrl)
            {
                Content =
                    new StringContent(
                        payload,
                        Encoding.UTF8,
                        "application/json")
            };

        request.Headers.Add(
            "Stripe-Signature",
            signature);

        return request;
    }

    private sealed class RecordingPaymentWebhookParser
        : IPaymentWebhookParser
    {
        public string? Payload { get; private set; }

        public string? Signature { get; private set; }

        public PaymentSubscriptionEvent? Parse(
            string payload,
            string signature)
        {
            Payload = payload;
            Signature = signature;

            return null;
        }
    }

    private sealed class InvalidPaymentWebhookParser
        : IPaymentWebhookParser
    {
        public PaymentSubscriptionEvent? Parse(
            string payload,
            string signature)
        {
            throw new InvalidPaymentWebhookException(
                "The webhook is invalid.");
        }
    }
}
