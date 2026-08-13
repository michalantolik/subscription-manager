using Microsoft.Extensions.Options;

namespace SubscriptionManager.Web.Common.FeatureToggles;

/// <summary>
/// Provides access to the current feature toggle state.
/// </summary>
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
