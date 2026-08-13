namespace SubscriptionManager.Application.ExchangeRates;

/// <summary>
/// Indicates that exchange rates required for currency conversion are unavailable.
/// </summary>
public sealed class ExchangeRatesUnavailableException
    : Exception
{
    public ExchangeRatesUnavailableException(
        string message)
        : base(message)
    {
    }

    public ExchangeRatesUnavailableException(
        string message,
        Exception innerException)
        : base(message, innerException)
    {
    }
}
