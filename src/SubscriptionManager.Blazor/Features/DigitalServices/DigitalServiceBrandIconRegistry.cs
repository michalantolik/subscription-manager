namespace SubscriptionManager.Blazor.Features.DigitalServices;

public static class DigitalServiceBrandIconRegistry
{
    private static readonly HashSet<string> IconKeys =
        new(StringComparer.OrdinalIgnoreCase)
        {
            "adobe",
            "allegro",
            "amazon-music",
            "amazon-prime",
            "apple-arcade",
            "apple-fitness",
            "apple-tv-plus",
            "audioteka",
            "audible",
            "badoo",
            "bolt",
            "bumble",
            "canva",
            "chatgpt",
            "cursor",
            "deezer",
            "discord",
            "dropbox",
            "duolingo",
            "ea",
            "empik",
            "empik-go",
            "evernote",
            "figma",
            "github-copilot",
            "glovo",
            "google-one",
            "google-play",
            "google-workspace",
            "hbo-max",
            "icloud",
            "jetbrains",
            "kindle",
            "legimi",
            "linkedin",
            "lyft",
            "microsoft-365",
            "monzo",
            "n26",
            "netflix",
            "nordvpn",
            "notion",
            "perplexity",
            "playstation",
            "reddit",
            "snapchat",
            "soundcloud",
            "spotify",
            "storytel",
            "strava",
            "telegram",
            "tinder",
            "uber",
            "wolt",
            "x",
            "xbox",
            "youtube",
            "youtube-tv",
        };

    public static bool Contains(
        string? iconKey)
    {
        return !string.IsNullOrWhiteSpace(iconKey) &&
               IconKeys.Contains(iconKey);
    }
}
