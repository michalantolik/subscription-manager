namespace SubscriptionManager.Application.SavingsPlans;

/// <summary>
/// Indicates that a savings plan is unavailable.
/// </summary>
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
