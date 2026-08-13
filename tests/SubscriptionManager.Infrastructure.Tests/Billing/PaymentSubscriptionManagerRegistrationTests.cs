using Microsoft.Extensions.DependencyInjection;
using SubscriptionManager.Application.Billing.PaymentProvider;
using SubscriptionManager.Infrastructure.Billing.Stripe;

namespace SubscriptionManager.Infrastructure.Tests.Billing;

public sealed class PaymentSubscriptionManagerRegistrationTests
{
    [Fact]
    public void AddInfrastructure_registers_payment_subscription_manager()
    {
        var serviceDescriptor =
            new ServiceCollection()
                .AddScoped<
                    IPaymentSubscriptionManager,
                    StripePaymentSubscriptionManager>()
                .Single(
                    descriptor =>
                        descriptor.ServiceType ==
                        typeof(IPaymentSubscriptionManager));

        Assert.Equal(
            typeof(StripePaymentSubscriptionManager),
            serviceDescriptor.ImplementationType);

        Assert.Equal(
            ServiceLifetime.Scoped,
            serviceDescriptor.Lifetime);
    }
}
