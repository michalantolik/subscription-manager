namespace SubscriptionManager.Web.Configuration;

public sealed class ApiOptions
{
    public const string SectionName = "Api";

    public string BaseUrl { get; init; } = string.Empty;
}
