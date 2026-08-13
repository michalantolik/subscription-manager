namespace SubscriptionManager.Web.Common.Localization;

/// <summary>
/// Provides culture-related operations for supported languages.
/// </summary>
public static class LanguageExtensions
{
    public static string ToCultureName(
        this Language language)
    {
        return language switch
        {
            Language.Polish => "pl-PL",
            Language.English => "en-US",
            Language.German => "de-DE",

            _ => throw new ArgumentOutOfRangeException(
                nameof(language),
                "The language is not supported.")
        };
    }
}
