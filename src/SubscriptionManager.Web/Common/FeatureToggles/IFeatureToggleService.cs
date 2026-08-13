namespace SubscriptionManager.Web.Common.FeatureToggles;

/// <summary>
/// Provides access to the current feature toggle state.
/// </summary>
public interface IFeatureToggleService
{
    bool IsEnabled(FeatureName featureName);
}
