namespace SubscriptionManager.Application.SavingsPlans;

public sealed class SavingsPlanUnavailableException
    : Exception
{
    public SavingsPlanUnavailableException(
        string message)
        : base(message)
    {
    }

    public SavingsPlanUnavailableException(
        string message,
        Exception innerException)
        : base(message, innerException)
    {
    }
}
