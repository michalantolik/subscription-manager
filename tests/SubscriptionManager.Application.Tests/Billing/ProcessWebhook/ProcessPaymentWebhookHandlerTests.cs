using Moq;
using SubscriptionManager.Application.Billing.ProcessWebhook;
using SubscriptionManager.Domain.Billing;

namespace SubscriptionManager.Application.Tests.Billing.ProcessWebhook;

public sealed class ProcessPaymentWebhookHandlerTests
{
    [Fact]
    public async Task HandleAsync_ShouldApplySupportedPaymentEvent()
    {
        const string payload =
            """{"id":"evt_123"}""";

        const string signature =
            "stripe-signature";

        var processedAt =
            new DateTimeOffset(
                2026,
                8,
                11,
                10,
                0,
                0,
                TimeSpan.Zero);

        var paymentEvent =
            CreatePaymentEvent();

        var parser =
            new Mock<IPaymentWebhookParser>();

        var repository =
            new Mock<IBillingWebhookRepository>();

        var timeProvider =
            new Mock<TimeProvider>();

        parser
            .Setup(currentParser =>
                currentParser.Parse(
                    payload,
                    signature))
            .Returns(
                paymentEvent);

        repository
            .Setup(currentRepository =>
                currentRepository.ApplyAsync(
                    paymentEvent,
                    processedAt,
                    It.IsAny<CancellationToken>()))
            .ReturnsAsync(
                PaymentWebhookProcessingResult.Applied);

        timeProvider
            .Setup(currentTimeProvider =>
                currentTimeProvider.GetUtcNow())
            .Returns(
                processedAt);

        var handler =
            new ProcessPaymentWebhookHandler(
                parser.Object,
                repository.Object,
                timeProvider.Object);

        var result =
            await handler.HandleAsync(
                new ProcessPaymentWebhookCommand(
                    payload,
                    signature));

        Assert.Equal(
            PaymentWebhookProcessingResult.Applied,
            result);

        parser.Verify(
            currentParser =>
                currentParser.Parse(
                    payload,
                    signature),
            Times.Once);

        repository.Verify(
            currentRepository =>
                currentRepository.ApplyAsync(
                    paymentEvent,
                    processedAt,
                    It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task HandleAsync_ShouldReturnIgnored_ForUnsupportedEventType()
    {
        const string payload =
            """{"id":"evt_unsupported"}""";

        const string signature =
            "stripe-signature";

        var parser =
            new Mock<IPaymentWebhookParser>();

        var repository =
            new Mock<IBillingWebhookRepository>();

        var timeProvider =
            new Mock<TimeProvider>();

        parser
            .Setup(currentParser =>
                currentParser.Parse(
                    payload,
                    signature))
            .Returns(
                (PaymentSubscriptionEvent?)null);

        var handler =
            new ProcessPaymentWebhookHandler(
                parser.Object,
                repository.Object,
                timeProvider.Object);

        var result =
            await handler.HandleAsync(
                new ProcessPaymentWebhookCommand(
                    payload,
                    signature));

        Assert.Equal(
            PaymentWebhookProcessingResult.Ignored,
            result);

        repository.Verify(
            currentRepository =>
                currentRepository.ApplyAsync(
                    It.IsAny<PaymentSubscriptionEvent>(),
                    It.IsAny<DateTimeOffset>(),
                    It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task HandleAsync_ShouldThrow_WhenPayloadIsMissing(
        string? payload)
    {
        var parser =
            new Mock<IPaymentWebhookParser>();

        var repository =
            new Mock<IBillingWebhookRepository>();

        var timeProvider =
            new Mock<TimeProvider>();

        var handler =
            new ProcessPaymentWebhookHandler(
                parser.Object,
                repository.Object,
                timeProvider.Object);

        await Assert.ThrowsAsync<
            InvalidPaymentWebhookException>(() =>
                handler.HandleAsync(
                    new ProcessPaymentWebhookCommand(
                        payload!,
                        "stripe-signature")));

        parser.Verify(
            currentParser =>
                currentParser.Parse(
                    It.IsAny<string>(),
                    It.IsAny<string>()),
            Times.Never);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task HandleAsync_ShouldThrow_WhenSignatureIsMissing(
        string? signature)
    {
        var parser =
            new Mock<IPaymentWebhookParser>();

        var repository =
            new Mock<IBillingWebhookRepository>();

        var timeProvider =
            new Mock<TimeProvider>();

        var handler =
            new ProcessPaymentWebhookHandler(
                parser.Object,
                repository.Object,
                timeProvider.Object);

        await Assert.ThrowsAsync<
            InvalidPaymentWebhookException>(() =>
                handler.HandleAsync(
                    new ProcessPaymentWebhookCommand(
                        """{"id":"evt_123"}""",
                        signature!)));

        parser.Verify(
            currentParser =>
                currentParser.Parse(
                    It.IsAny<string>(),
                    It.IsAny<string>()),
            Times.Never);
    }

    private static PaymentSubscriptionEvent CreatePaymentEvent()
    {
        var eventCreatedAt =
            new DateTimeOffset(
                2026,
                8,
                11,
                9,
                59,
                0,
                TimeSpan.Zero);

        var periodStart =
            new DateTimeOffset(
                2026,
                8,
                11,
                9,
                0,
                0,
                TimeSpan.Zero);

        return new PaymentSubscriptionEvent(
            "evt_123",
            eventCreatedAt,
            Guid.NewGuid(),
            "cus_123",
            "sub_123",
            "price_plus_monthly",
            SubscriptionPlan.Plus,
            BillingInterval.Monthly,
            BillingSubscriptionStatus.Active,
            periodStart,
            periodStart.AddMonths(1),
            false);
    }
}
