namespace SubscriptionManager.Blazor.Features.Localization;

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
