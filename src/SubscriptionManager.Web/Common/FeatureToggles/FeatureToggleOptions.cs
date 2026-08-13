namespace SubscriptionManager.Web.Common.FeatureToggles;

/// <summary>
/// Configuration for feature toggles in the web application.
/// </summary>
public sealed class FeatureToggleOptions
{
    public const string SectionName = "FeatureToggles";

    public bool PaidPlans { get; init; }
}
