using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SubscriptionManager.Blazor.Features.FeatureToggles;

namespace SubscriptionManager.Blazor.Tests;

public sealed class FeatureToggleConfigurationTests
{
    [Fact]
    public void IsEnabled_WhenPaidPlansAreMissing_ReturnsFalse()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(
                new Dictionary<string, string?>())
            .Build();

        var service = CreateService(configuration);

        var result = service.IsEnabled(
            FeatureName.PaidPlans);

        Assert.False(result);
    }

    [Fact]
    public void IsEnabled_WhenPaidPlansAreFalse_ReturnsFalse()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(
                new Dictionary<string, string?>
                {
                    ["FeatureToggles:PaidPlans"] = "false"
                })
            .Build();

        var service = CreateService(configuration);

        var result = service.IsEnabled(
            FeatureName.PaidPlans);

        Assert.False(result);
    }

    [Fact]
    public void IsEnabled_WhenPaidPlansAreTrue_ReturnsTrue()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(
                new Dictionary<string, string?>
                {
                    ["FeatureToggles:PaidPlans"] = "true"
                })
            .Build();

        var service = CreateService(configuration);

        var result = service.IsEnabled(
            FeatureName.PaidPlans);

        Assert.True(result);
    }

    private static IFeatureToggleService CreateService(
        IConfiguration configuration)
    {
        var services = new ServiceCollection();

        services
            .AddOptions<FeatureToggleOptions>()
            .Bind(
                configuration.GetSection(
                    FeatureToggleOptions.SectionName));

        services.AddSingleton<
            IFeatureToggleService,
            FeatureToggleService>();

        return services
            .BuildServiceProvider()
            .GetRequiredService<IFeatureToggleService>();
    }
}
