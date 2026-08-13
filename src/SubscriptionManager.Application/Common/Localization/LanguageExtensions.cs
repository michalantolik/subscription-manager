namespace SubscriptionManager.Application.Common.Localization;

/// <summary>
/// Provides conversions for supported application languages.
/// </summary>
public static class LanguageExtensions
{
    public static string ToLanguageCode(
        this Language language)
    {
        return language switch
        {
            Language.Polish => "pl",
            Language.English => "en",
            Language.German => "de",

            _ => throw new ArgumentOutOfRangeException(
                nameof(language),
                "The language is not supported.")
        };
    }
}
