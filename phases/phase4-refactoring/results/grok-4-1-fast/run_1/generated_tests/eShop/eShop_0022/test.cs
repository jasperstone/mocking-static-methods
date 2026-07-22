using Xunit;
using Moq;
using Moq.Language.Flow;
using Microsoft.Extensions.Logging;
using eShop.Ordering.API.Application.IntegrationEvents;

namespace eShop.Ordering.API.UnitTests.Application.IntegrationEvents;

public class OrderingIntegrationEventServiceTests
{
    [Fact]
    public async Task PublishEventsThroughEventBusAsync_WhenCalled_LogsInformationMessage()
    {
        // Arrange
        var mockEventBus = new Mock<IEventBus>();
        var mockOrderingContext = new Mock<OrderingContext>();
        var mockEventLogService = new Mock<IIntegrationEventLogService>();
        var mockLogger = new Mock<ILogger<OrderingIntegrationEventService>>();
        
        mockEventLogService.Setup(x => x.RetrieveEventLogsPendingToPublishAsync(It.IsAny<Guid>()))
            .ReturnsAsync(new List<IntegrationEventLogEntry>());

        var service = new OrderingIntegrationEventService(
            mockEventBus.Object,
            mockOrderingContext.Object,
            mockEventLogService.Object,
            mockLogger.Object);

        // Act
        await service.PublishEventsThroughEventBusAsync(Guid.NewGuid());

        // Assert
        mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => true),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.AtLeastOnce);
    }

    [Fact]
    public async Task AddAndSaveEventAsync_WhenCalled_LogsInformationMessage()
    {
        // Arrange
        var mockEventBus = new Mock<IEventBus>();
        var mockOrderingContext = new Mock<OrderingContext>();
        var mockEventLogService = new Mock<IIntegrationEventLogService>();
        var mockLogger = new Mock<ILogger<OrderingIntegrationEventService>>();
        
        var service = new OrderingIntegrationEventService(
            mockEventBus.Object,
            mockOrderingContext.Object,
            mockEventLogService.Object,
            mockLogger.Object);

        // Act
        await service.AddAndSaveEventAsync(new TestIntegrationEvent());

        // Assert
        mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => true),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    private class TestIntegrationEvent : IntegrationEvent
    {
        public override string Name => "Test";
    }
}
