using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using eShop.Ordering.API.Application.IntegrationEvents;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

public class OrderingIntegrationEventServiceTests
{
    [Fact]
    public async Task PublishEventsThroughEventBusAsync_LogsInformation()
    {
        // Arrange
        var eventBusMock = new Mock<IEventBus>();
        var integrationEventLogServiceMock = new Mock<IIntegrationEventLogService>();
        var loggerMock = new Mock<ILogger<OrderingIntegrationEventService>>();

        var transactionId = Guid.NewGuid();
        var integrationEvent = new IntegrationEvent { Id = Guid.NewGuid(), EventId = Guid.NewGuid() };
        var logEvent = new IntegrationEventLogEntry { EventId = integrationEvent.EventId, IntegrationEvent = integrationEvent };

        integrationEventLogServiceMock.Setup(x => x.RetrieveEventLogsPendingToPublishAsync(transactionId))
            .ReturnsAsync(new List<IntegrationEventLogEntry> { logEvent });

        var service = new OrderingIntegrationEventService(
            eventBusMock.Object,
            null, // OrderingContext is not available
            integrationEventLogServiceMock.Object,
            loggerMock.Object);

        // Act
        await service.PublishEventsThroughEventBusAsync(transactionId);

        // Assert
        loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Publishing integration event")),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
            Times.Once);
    }

    [Fact]
    public async Task AddAndSaveEventAsync_LogsInformation()
    {
        // Arrange
        var eventBusMock = new Mock<IEventBus>();
        var integrationEventLogServiceMock = new Mock<IIntegrationEventLogService>();
        var loggerMock = new Mock<ILogger<OrderingIntegrationEventService>>();

        var integrationEvent = new IntegrationEvent { Id = Guid.NewGuid() };

        var service = new OrderingIntegrationEventService(
            eventBusMock.Object,
            null, // OrderingContext is not available
            integrationEventLogServiceMock.Object,
            loggerMock.Object);

        // Act
        await service.AddAndSaveEventAsync(integrationEvent);

        // Assert
        loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Enqueuing integration event")),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
            Times.Once);
    }
}
