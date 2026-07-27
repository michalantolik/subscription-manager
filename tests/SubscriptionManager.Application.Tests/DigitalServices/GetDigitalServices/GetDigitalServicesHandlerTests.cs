using Moq;
using SubscriptionManager.Application.DigitalServices;
using SubscriptionManager.Application.DigitalServices.GetDigitalServices;
using SubscriptionManager.Domain.DigitalServices;

namespace SubscriptionManager.Application.Tests.DigitalServices
    .GetDigitalServices;

public sealed class GetDigitalServicesHandlerTests
{
    [Fact]
    public async Task HandleAsync_ShouldReturnMappedDigitalServices()
    {
        var netflixId = Guid.NewGuid();
        var spotifyId = Guid.NewGuid();
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
            DigitalService.CreatePredefined(
                spotifyId,
                "spotify",
                "Spotify",
                DigitalServiceCategory.Music,
                "spotify",
                "https://www.spotify.com/account",
                20,
                createdAt)
        };

        var repository = new Mock<IDigitalServiceRepository>();

        repository
            .Setup(x => x.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(digitalServices);

        var handler = new GetDigitalServicesHandler(repository.Object);

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
            spotify =>
            {
                Assert.Equal(spotifyId, spotify.Id);
                Assert.Equal("spotify", spotify.Key);
                Assert.Equal("Spotify", spotify.Name);
                Assert.True(spotify.IsPredefined);
                Assert.Equal(
                    DigitalServiceCategory.Music,
                    spotify.Category);
                Assert.Null(spotify.CustomCategoryName);
                Assert.Equal("spotify", spotify.IconKey);
                Assert.Equal(
                    "https://www.spotify.com/account",
                    spotify.ManagementUrl);
                Assert.True(spotify.IsActive);
                Assert.Equal(20, spotify.SortOrder);
            });
    }

    [Fact]
    public async Task HandleAsync_ShouldReturnEmptyCollection_WhenCatalogIsEmpty()
    {
        var repository = new Mock<IDigitalServiceRepository>();

        repository
            .Setup(x => x.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(Array.Empty<DigitalService>());

        var handler = new GetDigitalServicesHandler(repository.Object);

        var result = await handler.HandleAsync();

        Assert.Empty(result);
    }
}
