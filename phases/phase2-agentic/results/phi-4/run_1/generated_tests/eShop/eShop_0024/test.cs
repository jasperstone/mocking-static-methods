using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using eShop.Ordering.API.Application.IntegrationEvents;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace eShop.Ordering.API.Tests.IntegrationEvents
{
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

            var logEvents = new List<EventLog>
            {
                new EventLog { EventId = Guid.NewGuid(), IntegrationEvent = new IntegrationEvent { Id = Guid.NewGuid() } }
            };

            mockEventLogService
                .Setup(x => x.RetrieveEventLogsPendingToPublishAsync(It.IsAny<Guid>()))
                .ReturnsAsync(logEvents);

            var service = new OrderingIntegrationEventService(
                mockEventBus.Object,
                mockOrderingContext.Object,
                mockEventLogService.Object,
                mockLogger.Object);

            // Act
            await service.PublishEventsThroughEventBusAsync(Guid.NewGuid());

            // Assert
            foreach (var logEvent in logEvents)
            {
                mockLogger.Verify(
                    x => x.LogInformation(
                        It.Is<string>(s => s.Contains($"Publishing integration event: {logEvent.EventId}")),
                        It.Is<IntegrationEvent>(evt => evt.Id == logEvent.IntegrationEvent.Id),
                        It.IsAny<object[]>()),
                    Times.Once);
            }
        }
    }

    // Mock classes for testing
    public class EventLog
    {
        public Guid EventId { get; set; }
        public IntegrationEvent IntegrationEvent { get; set; }
    }

    public class IntegrationEvent
    {
        public Guid Id { get; set; }
    }
}
