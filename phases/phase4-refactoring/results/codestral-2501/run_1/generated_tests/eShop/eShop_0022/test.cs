using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using eShop.Ordering.API.Application.IntegrationEvents;
using eShop.Ordering.API.Application.IntegrationEvents.Events;

public class OrderingIntegrationEventServiceTests
{
    [Fact]
    public async Task PublishEventsThroughEventBusAsync_LogsInformation()
    {
        // Arrange
        var eventBusMock = new Mock<IEventBus>();
        var orderingContextMock = new Mock<OrderingContext>();
        var eventLogServiceMock = new Mock<IIntegrationEventLogService>();
        var loggerMock = new Mock<ILogger<OrderingIntegrationEventService>>();

        var transactionId = Guid.NewGuid();
        var pendingLogEvents = new List<IntegrationEventLogEntry>
        {
            new IntegrationEventLogEntry { EventId = Guid.NewGuid(), IntegrationEvent = new OrderStartedIntegrationEvent(Guid.NewGuid()) }
        };

        eventLogServiceMock.Setup(x => x.RetrieveEventLogsPendingToPublishAsync(transactionId))
            .ReturnsAsync(pendingLogEvents);

        var service = new OrderingIntegrationEventService(
            eventBusMock.Object,
            orderingContextMock.Object,
            eventLogServiceMock.Object,
            loggerMock.Object);

        // Act
        await service.PublishEventsThroughEventBusAsync(transactionId);

        // Assert
        loggerMock.Verify(
            x => x.Log(
                It.Is<LogLevel>(l => l == LogLevel.Information),
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => true),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
            Times.Once);
    }

    [Fact]
    public async Task AddAndSaveEventAsync_LogsInformation()
    {
        // Arrange
        var eventBusMock = new Mock<IEventBus>();
        var orderingContextMock = new Mock<OrderingContext>();
        var eventLogServiceMock = new Mock<IIntegrationEventLogService>();
        var loggerMock = new Mock<ILogger<OrderingIntegrationEventService>>();

        var integrationEvent = new OrderStartedIntegrationEvent(Guid.NewGuid());

        var service = new OrderingIntegrationEventService(
            eventBusMock.Object,
            orderingContextMock.Object,
            eventLogServiceMock.Object,
            loggerMock.Object);

        // Act
        await service.AddAndSaveEventAsync(integrationEvent);

        // Assert
        loggerMock.Verify(
            x => x.Log(
                It.Is<LogLevel>(l => l == LogLevel.Information),
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => true),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
            Times.Once);
    }
}
