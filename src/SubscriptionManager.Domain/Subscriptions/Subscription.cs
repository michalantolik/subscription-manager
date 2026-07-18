namespace SubscriptionManager.Domain.Subscriptions;

public sealed class Subscription
{
    private Subscription()
    {
    }

    public Subscription(
        Guid id,
        string name,
        decimal amount,
        string currency,
        BillingPeriod billingPeriod,
        DateOnly startDate)
    {
        if (id == Guid.Empty)
        {
            throw new ArgumentException(
                "Subscription identifier cannot be empty.",
                nameof(id));
        }

        Id = id;
        SetName(name);
        SetAmount(amount);
        SetCurrency(currency);
        SetBillingPeriod(billingPeriod);
        StartDate = startDate;
    }

    public Guid Id { get; private set; }

    public string Name { get; private set; } = string.Empty;

    public decimal Amount { get; private set; }

    public string Currency { get; private set; } = string.Empty;

    public BillingPeriod BillingPeriod { get; private set; }

    public DateOnly StartDate { get; private set; }

    public DateOnly? EndDate { get; private set; }

    public bool IsActive => EndDate is null;

    public decimal MonthlyEquivalentAmount =>
        BillingPeriod switch
        {
            BillingPeriod.Monthly => Amount,
            BillingPeriod.Quarterly => Amount / 3,
            BillingPeriod.SemiAnnual => Amount / 6,
            BillingPeriod.Yearly => Amount / 12,
            _ => throw new InvalidOperationException(
                "The billing period is not supported.")
        };

    public decimal YearlyEquivalentAmount =>
        MonthlyEquivalentAmount * 12;

    public void Update(
        string name,
        decimal amount,
        string currency,
        BillingPeriod billingPeriod)
    {
        SetName(name);
        SetAmount(amount);
        SetCurrency(currency);
        SetBillingPeriod(billingPeriod);
    }

    public void End(DateOnly endDate)
    {
        if (EndDate is not null)
        {
            throw new InvalidOperationException(
                "The subscription has already ended.");
        }

        if (endDate < StartDate)
        {
            throw new ArgumentOutOfRangeException(
                nameof(endDate),
                "The end date cannot be earlier than the start date.");
        }

        EndDate = endDate;
    }

    private void SetName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException(
                "Subscription name is required.",
                nameof(name));
        }

        Name = name.Trim();
    }

    private void SetAmount(decimal amount)
    {
        if (amount <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(amount),
                "Subscription amount must be greater than zero.");
        }

        Amount = amount;
    }

    private void SetCurrency(string currency)
    {
        if (string.IsNullOrWhiteSpace(currency) ||
            currency.Trim().Length != 3)
        {
            throw new ArgumentException(
                "Currency must be a three-letter code.",
                nameof(currency));
        }

        Currency = currency.Trim().ToUpperInvariant();
    }

    private void SetBillingPeriod(BillingPeriod billingPeriod)
    {
        if (!Enum.IsDefined(billingPeriod))
        {
            throw new ArgumentOutOfRangeException(
                nameof(billingPeriod),
                "The billing period is not supported.");
        }

        BillingPeriod = billingPeriod;
    }
}
