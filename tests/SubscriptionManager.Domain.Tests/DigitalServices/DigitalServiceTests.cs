using SubscriptionManager.Domain.DigitalServices;

namespace SubscriptionManager.Domain.Tests.DigitalServices;

public sealed class DigitalServiceTests
{
    [Fact]
    public void CreatePredefinedCreatesSystemManagedService()
    {
        var id = Guid.NewGuid();
        var createdAt = DateTimeOffset.UtcNow;

        var service = DigitalService.CreatePredefined(
            id,
            "netflix",
            "Netflix",
            DigitalServiceCategory.Video,
            "netflix",
            "https://www.netflix.com/account",
            10,
            createdAt);

        Assert.Equal(id, service.Id);
        Assert.Equal("netflix", service.Key);
        Assert.Equal("Netflix", service.Name);
        Assert.True(service.IsPredefined);
        Assert.Null(service.OwnerId);
        Assert.Equal(DigitalServiceCategory.Video, service.Category);
        Assert.Null(service.CustomCategoryName);
        Assert.Equal("netflix", service.IconKey);
        Assert.Equal(
            "https://www.netflix.com/account",
            service.ManagementUrl);
        Assert.True(service.IsActive);
        Assert.Equal(10, service.SortOrder);
        Assert.Equal(createdAt, service.CreatedAt);
    }

    [Fact]
    public void CreateCustomCreatesUserOwnedService()
    {
        var id = Guid.NewGuid();
        var ownerId = Guid.NewGuid();
        var createdAt = DateTimeOffset.UtcNow;

        var service = DigitalService.CreateCustom(
            id,
            ownerId,
            "my-streaming-service",
            "My Streaming Service",
            DigitalServiceCategory.Other,
            "Streaming",
            "custom-service",
            "https://example.com/account",
            createdAt);

        Assert.Equal(id, service.Id);
        Assert.Equal(ownerId, service.OwnerId);
        Assert.Equal("my-streaming-service", service.Key);
        Assert.Equal("My Streaming Service", service.Name);
        Assert.False(service.IsPredefined);
        Assert.Equal(DigitalServiceCategory.Other, service.Category);
        Assert.Equal("Streaming", service.CustomCategoryName);
        Assert.Equal("custom-service", service.IconKey);
        Assert.Equal(
            "https://example.com/account",
            service.ManagementUrl);
        Assert.True(service.IsActive);
        Assert.Equal(0, service.SortOrder);
        Assert.Equal(createdAt, service.CreatedAt);
    }

    [Fact]
    public void CreateCustomThrowsWhenOwnerIdIsEmpty()
    {
        var exception = Assert.Throws<ArgumentException>(() =>
            DigitalService.CreateCustom(
                Guid.NewGuid(),
                Guid.Empty,
                "my-service",
                "My Service",
                DigitalServiceCategory.Other,
                null,
                null,
                null,
                DateTimeOffset.UtcNow));

        Assert.Equal("ownerId", exception.ParamName);
    }

    [Fact]
    public void CreateCustomThrowsWhenCustomCategoryIsUsedWithKnownCategory()
    {
        var exception = Assert.Throws<ArgumentException>(() =>
            DigitalService.CreateCustom(
                Guid.NewGuid(),
                Guid.NewGuid(),
                "my-video-service",
                "My Video Service",
                DigitalServiceCategory.Video,
                "Streaming",
                null,
                null,
                DateTimeOffset.UtcNow));

        Assert.Equal("customCategoryName", exception.ParamName);
    }

    [Fact]
    public void CreatePredefinedThrowsWhenSortOrderIsNegative()
    {
        var exception = Assert.Throws<ArgumentOutOfRangeException>(() =>
            DigitalService.CreatePredefined(
                Guid.NewGuid(),
                "netflix",
                "Netflix",
                DigitalServiceCategory.Video,
                "netflix",
                "https://www.netflix.com/account",
                -1,
                DateTimeOffset.UtcNow));

        Assert.Equal("sortOrder", exception.ParamName);
    }

    [Fact]
    public void CreatePredefinedNormalizesKeyAndName()
    {
        var service = CreateDigitalService(
            key: "  NetFlix  ",
            name: "  Netflix  ");

        Assert.Equal("netflix", service.Key);
        Assert.Equal("Netflix", service.Name);
    }

    [Fact]
    public void CreatePredefinedNormalizesOptionalValues()
    {
        var service = CreateDigitalService(
            iconKey: "   ",
            managementUrl: "   ");

        Assert.Null(service.IconKey);
        Assert.Null(service.ManagementUrl);
    }

    [Fact]
    public void CreatePredefinedTrimsOptionalValues()
    {
        var service = CreateDigitalService(
            iconKey: "  netflix  ",
            managementUrl: "  https://www.netflix.com/account  ");

        Assert.Equal("netflix", service.IconKey);
        Assert.Equal(
            "https://www.netflix.com/account",
            service.ManagementUrl);
    }

    [Fact]
    public void CreatePredefinedThrowsWhenIdIsEmpty()
    {
        var exception = Assert.Throws<ArgumentException>(() =>
            CreateDigitalService(id: Guid.Empty));

        Assert.Equal("id", exception.ParamName);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void CreatePredefinedThrowsWhenKeyIsEmpty(string key)
    {
        var exception = Assert.Throws<ArgumentException>(() =>
            CreateDigitalService(key: key));

        Assert.Equal("key", exception.ParamName);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void CreatePredefinedThrowsWhenNameIsEmpty(string name)
    {
        var exception = Assert.Throws<ArgumentException>(() =>
            CreateDigitalService(name: name));

        Assert.Equal("name", exception.ParamName);
    }

    [Theory]
    [InlineData("netflix.com")]
    [InlineData("invalid-url")]
    [InlineData("ftp://netflix.com")]
    public void CreatePredefinedThrowsWhenManagementUrlIsInvalid(
        string managementUrl)
    {
        var exception = Assert.Throws<ArgumentException>(() =>
            CreateDigitalService(managementUrl: managementUrl));

        Assert.Equal("managementUrl", exception.ParamName);
    }

    private static DigitalService CreateDigitalService(
        Guid? id = null,
        string key = "netflix",
        string name = "Netflix",
        DigitalServiceCategory category = DigitalServiceCategory.Video,
        string? iconKey = "netflix",
        string? managementUrl = "https://www.netflix.com/account",
        DateTimeOffset? createdAt = null)
    {
        return DigitalService.CreatePredefined(
            id ?? Guid.NewGuid(),
            key,
            name,
            category,
            iconKey,
            managementUrl,
            sortOrder: 10,
            createdAt: createdAt ?? DateTimeOffset.UtcNow);
    }
}
