using Microsoft.Extensions.Options;
using SubscriptionManager.Blazor.Features.FeatureToggles;

namespace SubscriptionManager.Blazor.Tests;

public sealed class FeatureToggleServiceTests
{
    [Fact]
    public void IsEnabled_WhenPaidPlansAreEnabled_ReturnsTrue()
    {
        var options = CreateOptions(
            paidPlans: true);

        var service = new FeatureToggleService(options);

        var result = service.IsEnabled(
            FeatureName.PaidPlans);

        Assert.True(result);
    }

    [Fact]
    public void IsEnabled_WhenPaidPlansAreDisabled_ReturnsFalse()
    {
        var options = CreateOptions(
            paidPlans: false);

        var service = new FeatureToggleService(options);

        var result = service.IsEnabled(
            FeatureName.PaidPlans);

        Assert.False(result);
    }

    private static IOptionsMonitor<FeatureToggleOptions> CreateOptions(
        bool paidPlans)
    {
        return new TestOptionsMonitor(
            new FeatureToggleOptions
            {
                PaidPlans = paidPlans
            });
    }

    private sealed class TestOptionsMonitor(
        FeatureToggleOptions currentValue)
        : IOptionsMonitor<FeatureToggleOptions>
    {
        public FeatureToggleOptions CurrentValue =>
            currentValue;

        public FeatureToggleOptions Get(string? name)
        {
            return currentValue;
        }

        public IDisposable? OnChange(
            Action<FeatureToggleOptions, string?> listener)
        {
            return null;
        }
    }
}
