namespace SubscriptionManager.Infrastructure.SavingsPlans;

public sealed class SavingsPlanAiOptions
{
    public const string SectionName = "SavingsPlanAi";

    public string Endpoint { get; init; } =
        "https://api.openai.com/v1";

    public string ApiKey { get; init; } =
        string.Empty;

    public string Model { get; init; } =
        string.Empty;

    public int MaximumIterations { get; init; } = 8;

    public int MaximumOutputTokens { get; init; } = 800;

    public int RequestTimeoutSeconds { get; init; } = 30;
}
