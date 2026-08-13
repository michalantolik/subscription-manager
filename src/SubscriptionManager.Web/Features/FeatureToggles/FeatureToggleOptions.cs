namespace SubscriptionManager.Web.Features.FeatureToggles;

public sealed class FeatureToggleOptions
{
    public const string SectionName = "FeatureToggles";

    public bool PaidPlans { get; init; }
}
