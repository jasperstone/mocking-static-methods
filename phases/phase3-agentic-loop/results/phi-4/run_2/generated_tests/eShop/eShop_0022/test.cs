using Moq;
using Microsoft.Extensions.Logging;
using eShop.Ordering.API.Application.IntegrationEvents;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

public class OrderingIntegrationEventServiceTests
{
    [Fact]
    public async Task PublishEventsThroughEventBusAsync_LogsInformationForEachEvent()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<OrderingIntegrationEventService>>();

        var service = new OrderingIntegrationEventService(
            null, // Mock IEventBus as null
            null, // Mock OrderingContext as null
            null, // Mock IIntegrationEventLogService as null
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
