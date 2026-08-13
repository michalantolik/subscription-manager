using Moq;
using SubscriptionManager.Application.Billing.CreateCheckoutSession;
using SubscriptionManager.Application.Billing.PaymentProvider;
using SubscriptionManager.Application.Common.Identity;
using SubscriptionManager.Domain.Billing;

namespace SubscriptionManager.Application.Tests.Billing.CreateCheckoutSession;

public sealed class CreateCheckoutSessionHandlerTests
{
    [Fact]
    public async Task HandleAsync_ShouldCreateCheckoutSessionForCurrentUser()
    {
        var userId = Guid.NewGuid();
        var email = "user@example.com";

        var checkoutUrl =
            new Uri("https://checkout.example.com/session");

        var currentUser =
            new Mock<ICurrentUser>();

        var identityService =
            new Mock<IIdentityService>();

        var paymentProvider =
            new Mock<IPaymentProvider>();

        currentUser
            .SetupGet(user =>
                user.UserId)
            .Returns(userId);

        identityService
            .Setup(service =>
                service.GetEmailAsync(
                    userId,
                    It.IsAny<CancellationToken>()))
            .ReturnsAsync(email);

        paymentProvider
            .Setup(provider =>
                provider.CreateCheckoutSessionAsync(
                    userId,
                    email,
                    SubscriptionPlan.Plus,
                    BillingInterval.Monthly,
                    new Uri("https://app.example.com/billing/success"),
                    new Uri("https://app.example.com/billing/cancel"),
                    It.IsAny<CancellationToken>()))
            .ReturnsAsync(checkoutUrl);

        var handler =
            new CreateCheckoutSessionHandler(
                currentUser.Object,
                identityService.Object,
                paymentProvider.Object);

        var command =
            new CreateCheckoutSessionCommand(
                SubscriptionPlan.Plus,
                BillingInterval.Monthly,
                "https://app.example.com/billing/success",
                "https://app.example.com/billing/cancel");

        var result =
            await handler.HandleAsync(command);

        Assert.Equal(
            checkoutUrl,
            result);

        identityService.Verify(
            service =>
                service.GetEmailAsync(
                    userId,
                    It.IsAny<CancellationToken>()),
            Times.Once);

        paymentProvider.Verify(
            provider =>
                provider.CreateCheckoutSessionAsync(
                    userId,
                    email,
                    SubscriptionPlan.Plus,
                    BillingInterval.Monthly,
                    new Uri("https://app.example.com/billing/success"),
                    new Uri("https://app.example.com/billing/cancel"),
                    It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task HandleAsync_ShouldReturnNull_WhenUserEmailDoesNotExist()
    {
        var userId = Guid.NewGuid();

        var currentUser =
            new Mock<ICurrentUser>();

        var identityService =
            new Mock<IIdentityService>();

        var paymentProvider =
            new Mock<IPaymentProvider>();

        currentUser
            .SetupGet(user =>
                user.UserId)
            .Returns(userId);

        identityService
            .Setup(service =>
                service.GetEmailAsync(
                    userId,
                    It.IsAny<CancellationToken>()))
            .ReturnsAsync(
                (string?)null);

        var handler =
            new CreateCheckoutSessionHandler(
                currentUser.Object,
                identityService.Object,
                paymentProvider.Object);

        var command =
            new CreateCheckoutSessionCommand(
                SubscriptionPlan.Plus,
                BillingInterval.Monthly,
                "https://app.example.com/billing/success",
                "https://app.example.com/billing/cancel");

        var result =
            await handler.HandleAsync(command);

        Assert.Null(result);

        paymentProvider.Verify(
            provider =>
                provider.CreateCheckoutSessionAsync(
                    It.IsAny<Guid>(),
                    It.IsAny<string>(),
                    It.IsAny<SubscriptionPlan>(),
                    It.IsAny<BillingInterval>(),
                    It.IsAny<Uri>(),
                    It.IsAny<Uri>(),
                    It.IsAny<CancellationToken>()),
            Times.Never);
    }
}
