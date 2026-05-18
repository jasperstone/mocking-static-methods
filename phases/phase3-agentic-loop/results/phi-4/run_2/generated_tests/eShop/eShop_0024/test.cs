using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using eShop.EventBus.Abstractions;
using eShop.IntegrationEventLogEF.Services;
using eShop.Ordering.API.Application.IntegrationEvents;
using eShop.Ordering.Infrastructure;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

public class OrderingIntegrationEventServiceTests
{
    [Fact]
    public async Task PublishEventsThroughEventBusAsync_LogsInformationForEachEvent()
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

        var transactionId = Guid.NewGuid();
        var logEvents = new List<IntegrationEventLog>
        {
            new IntegrationEventLog
            {
                EventId = Guid.NewGuid(),
                IntegrationEvent = new IntegrationEvent(Guid.NewGuid(), "TestEvent")
            }
        };

        mockEventLogService
            .Setup(x => x.RetrieveEventLogsPendingToPublishAsync(transactionId))
            .ReturnsAsync(logEvents);

        // Act
        await service.PublishEventsThroughEventBusAsync(transactionId);

        // Assert
        mockLogger.Verify(
            x => x.LogInformation(
                It.Is<string>(s => s.Contains("Publishing integration event")),
                It.IsAny<Guid>(),
                It.IsAny<IntegrationEvent>()), Times.Exactly(logEvents.Count));
    }
}

public class IntegrationEvent
{
    public Guid Id { get; }
    public string Name { get; }

    public IntegrationEvent(Guid id, string name)
    {
        Id = id;
        Name = name;
    }
}

public class IntegrationEventLog
{
    public Guid EventId { get; set; }
    public IntegrationEvent IntegrationEvent { get; set; }
}
