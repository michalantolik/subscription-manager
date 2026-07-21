using System.Globalization;

namespace SubscriptionManager.Blazor.Services;

public static class SupportedCultures
{
    public const string DefaultCulture = "pl-PL";

    public static readonly IReadOnlyList<CultureInfo> All =
    [
        CultureInfo.GetCultureInfo("pl-PL"),
        CultureInfo.GetCultureInfo("en-US"),
        CultureInfo.GetCultureInfo("de-DE")
    ];

    public static readonly IReadOnlyDictionary<string, string> ByLanguageCode =
        new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["pl"] = "pl-PL",
            ["en"] = "en-US",
            ["de"] = "de-DE"
        };

    public static bool Contains(string cultureName) =>
        All.Any(culture => string.Equals(culture.Name, cultureName, StringComparison.OrdinalIgnoreCase));
}
