namespace SubscriptionManager.Web.Common.Api;

/// <summary>
/// Configuration for accessing the Subscription Manager API.
/// </summary>
public sealed class ApiOptions
{
    public const string SectionName = "Api";

    public string BaseUrl { get; init; } = string.Empty;
}
