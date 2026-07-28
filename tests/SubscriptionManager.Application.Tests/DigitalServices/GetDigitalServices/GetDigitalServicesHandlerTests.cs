using Moq;
using SubscriptionManager.Application.Common.Identity;
using SubscriptionManager.Application.DigitalServices;
using SubscriptionManager.Application.DigitalServices.GetDigitalServices;
using SubscriptionManager.Domain.DigitalServices;

namespace SubscriptionManager.Application.Tests.DigitalServices
    .GetDigitalServices;

public sealed class GetDigitalServicesHandlerTests
{
    [Fact]
    public async Task HandleAsync_ShouldReturnAvailableDigitalServicesForCurrentUser()
    {
        var ownerId = Guid.NewGuid();
        var netflixId = Guid.NewGuid();
        var customServiceId = Guid.NewGuid();
        var createdAt = DateTimeOffset.UtcNow;

        var digitalServices = new[]
        {
            DigitalService.CreatePredefined(
                netflixId,
                "netflix",
                "Netflix",
                DigitalServiceCategory.Video,
                "netflix",
                "https://www.netflix.com/account",
                10,
                createdAt),
            DigitalService.CreateCustom(
                customServiceId,
                ownerId,
                "my-streaming-service",
                "My Streaming Service",
                DigitalServiceCategory.Other,
                "Streaming",
                "custom-service",
                "https://example.com/account",
                createdAt)
        };

        var repository = new Mock<IDigitalServiceRepository>();
        var currentUser = new Mock<ICurrentUser>();

        currentUser
            .SetupGet(x => x.UserId)
            .Returns(ownerId);

        repository
            .Setup(x => x.GetAvailableAsync(
                ownerId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(digitalServices);

        var handler = new GetDigitalServicesHandler(
            repository.Object,
            currentUser.Object);

        var result = await handler.HandleAsync();

        Assert.Collection(
            result,
            netflix =>
            {
                Assert.Equal(netflixId, netflix.Id);
                Assert.Equal("netflix", netflix.Key);
                Assert.Equal("Netflix", netflix.Name);
                Assert.True(netflix.IsPredefined);
                Assert.Equal(
                    DigitalServiceCategory.Video,
                    netflix.Category);
                Assert.Null(netflix.CustomCategoryName);
                Assert.Equal("netflix", netflix.IconKey);
                Assert.Equal(
                    "https://www.netflix.com/account",
                    netflix.ManagementUrl);
                Assert.True(netflix.IsActive);
                Assert.Equal(10, netflix.SortOrder);
            },
            customService =>
            {
                Assert.Equal(customServiceId, customService.Id);
                Assert.Equal(
                    "my-streaming-service",
                    customService.Key);
                Assert.Equal(
                    "My Streaming Service",
                    customService.Name);
                Assert.False(customService.IsPredefined);
                Assert.Equal(
                    DigitalServiceCategory.Other,
                    customService.Category);
                Assert.Equal(
                    "Streaming",
                    customService.CustomCategoryName);
                Assert.Equal(
                    "custom-service",
                    customService.IconKey);
                Assert.Equal(
                    "https://example.com/account",
                    customService.ManagementUrl);
                Assert.True(customService.IsActive);
                Assert.Equal(0, customService.SortOrder);
            });

        repository.Verify(
            x => x.GetAvailableAsync(
                ownerId,
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task HandleAsync_ShouldReturnEmptyCollection_WhenCurrentUserHasNoAvailableServices()
    {
        var ownerId = Guid.NewGuid();
        var repository = new Mock<IDigitalServiceRepository>();
        var currentUser = new Mock<ICurrentUser>();

        currentUser
            .SetupGet(x => x.UserId)
            .Returns(ownerId);

        repository
            .Setup(x => x.GetAvailableAsync(
                ownerId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Array.Empty<DigitalService>());

        var handler = new GetDigitalServicesHandler(
            repository.Object,
            currentUser.Object);

        var result = await handler.HandleAsync();

        Assert.Empty(result);

        repository.Verify(
            x => x.GetAvailableAsync(
                ownerId,
                It.IsAny<CancellationToken>()),
            Times.Once);
    }
}
