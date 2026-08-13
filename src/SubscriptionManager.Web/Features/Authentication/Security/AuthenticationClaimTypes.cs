namespace SubscriptionManager.Web.Features.Authentication.Security;

/// <summary>
/// Defines authentication claim types used by the web application.
/// </summary>
public static class AuthenticationClaimTypes
{
    public const string AccessToken =
        "subscription_manager:access_token";

    public const string SubscriptionPlan =
        "subscription_manager:subscription_plan";
}
