namespace SubscriptionManager.Blazor.Features.FeatureToggles;

public interface IFeatureToggleService
{
    bool IsEnabled(FeatureName featureName);
}
