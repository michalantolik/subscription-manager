namespace SubscriptionManager.Blazor.Configuration;

public sealed class ApiOptions
{
    public const string SectionName = "Api";
    public string BaseUrl { get; init; } = "http://localhost:5159";
}
