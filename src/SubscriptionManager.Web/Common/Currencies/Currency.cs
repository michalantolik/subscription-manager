using System.Text.Json.Serialization;

namespace SubscriptionManager.Web.Common.Currencies;

/// <summary>
/// Represents a currency supported by the web application.
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum Currency
{
    PLN = 1,
    EUR = 2,
    USD = 3,
    GBP = 4,
    CHF = 5,
    CZK = 6,
    SEK = 7,
    NOK = 8,
    DKK = 9
}
