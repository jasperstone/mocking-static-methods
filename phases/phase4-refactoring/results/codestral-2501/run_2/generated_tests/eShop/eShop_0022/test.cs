using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using eShop.Ordering.API.Application.IntegrationEvents;
using eShop.Ordering.API.Application.IntegrationEvents.EventHandling;
using eShop.Ordering.API.Application.IntegrationEvents.Events;

namespace eShop.Ordering.API.Application.IntegrationEvents.Tests
{
    public class OrderingIntegrationEventServiceTests
    {
        [Fact]
        public async Task PublishEventsThroughEventBusAsync_ShouldLogInformation()
        {
            // Arrange
            var eventBusMock = new Mock<IEventBus>();
            var orderingContextMock = new Mock<OrderingContext>();
            var integrationEventLogServiceMock = new Mock<IIntegrationEventLogService>();
            var loggerMock = new Mock<ILogger<OrderingIntegrationEventService>>();

            var transactionId = Guid.NewGuid();
            var integrationEvent = new IntegrationEvent { Id = Guid.NewGuid(), EventType = "TestEvent" };
            var integrationEventLog = new IntegrationEventLogEntry { EventId = Guid.NewGuid(), IntegrationEvent = integrationEvent };

            integrationEventLogServiceMock
                .Setup(x => x.RetrieveEventLogsPendingToPublishAsync(transactionId))
                .ReturnsAsync(new List<IntegrationEventLogEntry> { integrationEventLog });

            var service = new OrderingIntegrationEventService(
                eventBusMock.Object,
                orderingContextMock.Object,
                integrationEventLogServiceMock.Object,
                loggerMock.Object);

            // Act
            await service.PublishEventsThroughEventBusAsync(transactionId);

            // Assert
            loggerMock.Verify(
                x => x.Log(
                    It.Is<LogLevel>(l => l == LogLevel.Information),
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Publishing integration event")),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
                Times.Once);
        }
    }
}
