using SubscriptionManager.Domain.SavingsPlans;

namespace SubscriptionManager.Domain.Tests.SavingsPlans;

public sealed class SavingsPlanUsageTests
{
    [Fact]
    public void Constructor_ShouldCreateUsageForUserAndDate()
    {
        var userId = Guid.NewGuid();
        var usageDateUtc = new DateOnly(2026, 8, 6);

        var usage = new SavingsPlanUsage(
            userId,
            usageDateUtc);

        Assert.Equal(userId, usage.UserId);
        Assert.Equal(usageDateUtc, usage.UsageDateUtc);
        Assert.Equal(0, usage.RequestCount);
    }

    [Fact]
    public void Constructor_ShouldRejectEmptyUserId()
    {
        Assert.Throws<ArgumentException>(
            () => new SavingsPlanUsage(
                Guid.Empty,
                new DateOnly(2026, 8, 6)));
    }

    [Fact]
    public void Constructor_ShouldRejectDefaultUsageDate()
    {
        Assert.Throws<ArgumentException>(
            () => new SavingsPlanUsage(
                Guid.NewGuid(),
                default));
    }

    [Fact]
    public void RegisterRequest_ShouldIncreaseRequestCount()
    {
        var usage = CreateUsage();

        usage.RegisterRequest(dailyLimit: 3);

        Assert.Equal(1, usage.RequestCount);
    }

    [Fact]
    public void RegisterRequest_ShouldAllowRequestsUpToDailyLimit()
    {
        var usage = CreateUsage();

        usage.RegisterRequest(dailyLimit: 3);
        usage.RegisterRequest(dailyLimit: 3);
        usage.RegisterRequest(dailyLimit: 3);

        Assert.Equal(3, usage.RequestCount);
        Assert.True(usage.HasReachedLimit(dailyLimit: 3));
    }

    [Fact]
    public void RegisterRequest_ShouldRejectRequestAfterDailyLimit()
    {
        var usage = CreateUsage();

        usage.RegisterRequest(dailyLimit: 3);
        usage.RegisterRequest(dailyLimit: 3);
        usage.RegisterRequest(dailyLimit: 3);

        Assert.Throws<InvalidOperationException>(
            () => usage.RegisterRequest(dailyLimit: 3));
    }

    [Fact]
    public void GetRemainingRequestCount_ShouldReturnRemainingRequests()
    {
        var usage = CreateUsage();

        usage.RegisterRequest(dailyLimit: 3);

        var remaining =
            usage.GetRemainingRequestCount(dailyLimit: 3);

        Assert.Equal(2, remaining);
    }

    [Fact]
    public void GetRemainingRequestCount_ShouldNotReturnNegativeValue()
    {
        var usage = CreateUsage();

        usage.RegisterRequest(dailyLimit: 3);
        usage.RegisterRequest(dailyLimit: 3);
        usage.RegisterRequest(dailyLimit: 3);

        var remaining =
            usage.GetRemainingRequestCount(dailyLimit: 3);

        Assert.Equal(0, remaining);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void LimitOperations_ShouldRejectInvalidDailyLimit(
        int dailyLimit)
    {
        var usage = CreateUsage();

        Assert.Throws<ArgumentOutOfRangeException>(
            () => usage.HasReachedLimit(dailyLimit));

        Assert.Throws<ArgumentOutOfRangeException>(
            () => usage.GetRemainingRequestCount(dailyLimit));

        Assert.Throws<ArgumentOutOfRangeException>(
            () => usage.RegisterRequest(dailyLimit));
    }

    private static SavingsPlanUsage CreateUsage()
    {
        return new SavingsPlanUsage(
            Guid.NewGuid(),
            new DateOnly(2026, 8, 6));
    }
}
