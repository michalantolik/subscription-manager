using Moq;
using SubscriptionManager.Application.Common.Authentication;
using SubscriptionManager.Application.DigitalServices;
using SubscriptionManager.Application.DigitalServices.CreateDigitalService;
using SubscriptionManager.Domain.DigitalServices;

namespace SubscriptionManager.Application.Tests.DigitalServices.CreateDigitalService;

public sealed class CreateDigitalServiceHandlerTests
{
    [Fact]
    public async Task HandleAsync_ShouldCreateCustomDigitalServiceForCurrentUser()
    {
        var ownerId = Guid.NewGuid();
        var repository = new Mock<IDigitalServiceRepository>();
        var currentUser = new Mock<ICurrentUser>();

        currentUser
            .SetupGet(x => x.UserId)
            .Returns(ownerId);

        DigitalService? addedDigitalService = null;

        repository
            .Setup(x => x.AddAsync(
                It.IsAny<DigitalService>(),
                It.IsAny<CancellationToken>()))
            .Callback<DigitalService, CancellationToken>(
                (digitalService, _) => addedDigitalService = digitalService)
            .Returns(Task.CompletedTask);

        var handler = new CreateDigitalServiceHandler(
            repository.Object,
            currentUser.Object);

        var result = await handler.HandleAsync(
            new CreateDigitalServiceCommand(
                "my-service",
                "My Service",
                DigitalServiceCategory.Other,
                "Streaming",
                "custom-service",
                "https://example.com/account"));

        Assert.NotEqual(Guid.Empty, result);
        Assert.NotNull(addedDigitalService);
        Assert.Equal(result, addedDigitalService.Id);
        Assert.Equal(ownerId, addedDigitalService.OwnerId);
        Assert.False(addedDigitalService.IsPredefined);
        Assert.Equal("my-service", addedDigitalService.Key);
        Assert.Equal("My Service", addedDigitalService.Name);
        Assert.Equal(
            DigitalServiceCategory.Other,
            addedDigitalService.Category);
        Assert.Equal(
            "Streaming",
            addedDigitalService.CustomCategoryName);
        Assert.Equal("custom-service", addedDigitalService.IconKey);
        Assert.Equal(
            "https://example.com/account",
            addedDigitalService.ManagementUrl);
        Assert.True(addedDigitalService.IsActive);
        Assert.Equal(0, addedDigitalService.SortOrder);

        repository.Verify(
            x => x.AddAsync(
                addedDigitalService,
                It.IsAny<CancellationToken>()),
            Times.Once);

        repository.Verify(
            x => x.SaveChangesAsync(
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task HandleAsync_ShouldThrow_WhenCommandIsNull()
    {
        var repository = new Mock<IDigitalServiceRepository>();
        var currentUser = new Mock<ICurrentUser>();

        var handler = new CreateDigitalServiceHandler(
            repository.Object,
            currentUser.Object);

        await Assert.ThrowsAsync<ArgumentNullException>(() =>
            handler.HandleAsync(null!));

        repository.Verify(
            x => x.AddAsync(
                It.IsAny<DigitalService>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }
}
