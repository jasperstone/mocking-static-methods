using Xunit;
using Moq;
using eShop.Ordering.API.Application.IntegrationEvents;
using Microsoft.Extensions.Logging;
using eShop.Ordering.API.Infrastructure;
using System.Collections.Generic;

public class OrderingIntegrationEventServiceTests
{
    [Fact]
    public async Task AddAndSaveEventAsync_LogsInformation()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<OrderingIntegrationEventService>>();
        var eventLogServiceMock = new Mock<IIntegrationEventLogService>();
        var orderingContextMock = new Mock<OrderingContext>();
        var eventBusMock = new Mock<IEventBus>();
        var integrationEvent = new IntegrationEvent { Id = Guid.NewGuid() };

        var service = new OrderingIntegrationEventService(eventBusMock.Object, orderingContextMock.Object, eventLogServiceMock.Object, loggerMock.Object);

        // Act
        await service.AddAndSaveEventAsync(integrationEvent);

        // Assert
        loggerMock.Verify(l => l.LogInformation(It.IsAny<string>(), It.IsAny<object[]>()), Times.Once);
    }

    [Fact]
    public async Task PublishEventsThroughEventBusAsync_LogsInformation()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<OrderingIntegrationEventService>>();
        var eventLogServiceMock = new Mock<IIntegrationEventLogService>();
        var orderingContextMock = new Mock<OrderingContext>();
        var eventBusMock = new Mock<IEventBus>();
        var integrationEvent = new IntegrationEvent { Id = Guid.NewGuid() };

        eventLogServiceMock.Setup(es => es.RetrieveEventLogsPendingToPublishAsync(It.IsAny<Guid>())).ReturnsAsync(new List<IntegrationEventLog> { new IntegrationEventLog { EventId = integrationEvent.Id, IntegrationEvent = integrationEvent } });

        var service = new OrderingIntegrationEventService(eventBusMock.Object, orderingContextMock.Object, eventLogServiceMock.Object, loggerMock.Object);

        // Act
        await service.PublishEventsThroughEventBusAsync(Guid.NewGuid());

        // Assert
        loggerMock.Verify(l => l.LogInformation(It.IsAny<string>(), It.IsAny<object[]>()), Times.Once);
    }
}
