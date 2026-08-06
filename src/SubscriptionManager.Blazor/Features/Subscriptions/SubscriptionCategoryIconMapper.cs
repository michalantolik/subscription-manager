namespace SubscriptionManager.Blazor.Features.Subscriptions;

public static class SubscriptionCategoryIconMapper
{
    public static string GetIcon(
        string category)
    {
        return category.Trim().ToLowerInvariant() switch
        {
            "ai" => "ai",
            "video" => "video",
            "music" => "music",
            "gaming" => "gaming",
            "development" => "development",
            "cloud" => "cloud",
            "education" => "education",
            "finance" => "finance",
            _ => "other"
        };
    }
}
