using SubscriptionManager.Domain.Subscriptions;

namespace SubscriptionManager.Domain.ExchangeRates;

public sealed class ExchangeRate
{
    private ExchangeRate()
    {
    }

    public ExchangeRate(
        Currency currency,
        decimal rateToPln,
        DateOnly effectiveDate,
        DateTimeOffset lastCheckedAt)
    {
        if (!Enum.IsDefined(currency))
        {
            throw new ArgumentOutOfRangeException(
                nameof(currency),
                "The currency is not supported.");
        }

        if (currency == Currency.PLN)
        {
            throw new ArgumentException(
                "An exchange rate for PLN is not required.",
                nameof(currency));
        }

        Currency = currency;

        Update(
            rateToPln,
            effectiveDate,
            lastCheckedAt);
    }

    public Currency Currency { get; private set; }

    public decimal RateToPln { get; private set; }

    public DateOnly EffectiveDate { get; private set; }

    public DateTimeOffset LastCheckedAt { get; private set; }

    public void Update(
        decimal rateToPln,
        DateOnly effectiveDate,
        DateTimeOffset checkedAt)
    {
        if (rateToPln <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(rateToPln),
                "The exchange rate must be greater than zero.");
        }

        if (effectiveDate == default)
        {
            throw new ArgumentException(
                "The effective date is required.",
                nameof(effectiveDate));
        }

        if (checkedAt == default)
        {
            throw new ArgumentException(
                "The check date is required.",
                nameof(checkedAt));
        }

        RateToPln = rateToPln;
        EffectiveDate = effectiveDate;
        LastCheckedAt = checkedAt;
    }

    public void MarkAsChecked(
        DateTimeOffset checkedAt)
    {
        if (checkedAt == default)
        {
            throw new ArgumentException(
                "The check date is required.",
                nameof(checkedAt));
        }

        LastCheckedAt = checkedAt;
    }
}
