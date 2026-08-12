using Microsoft.Extensions.Options;

namespace SubscriptionManager.Blazor.Features.FeatureToggles;

public sealed class FeatureToggleService(
    IOptionsMonitor<FeatureToggleOptions> options) : IFeatureToggleService
{
    public bool IsEnabled(FeatureName featureName)
    {
        return featureName switch
        {
            FeatureName.PaidPlans => options.CurrentValue.PaidPlans,
            _ => false
        };
    }
}
