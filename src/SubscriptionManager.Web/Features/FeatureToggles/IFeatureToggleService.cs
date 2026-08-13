namespace SubscriptionManager.Web.Features.FeatureToggles;

public interface IFeatureToggleService
{
    bool IsEnabled(FeatureName featureName);
}
