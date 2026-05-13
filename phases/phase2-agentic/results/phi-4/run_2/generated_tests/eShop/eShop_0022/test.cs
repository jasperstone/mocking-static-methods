using Moq;
using Microsoft.Extensions.Logging;
using eShop.Ordering.API.Application.IntegrationEvents;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace eShop.Ordering.API.Tests
{
    public class OrderingIntegrationEventServiceTests
    {
        [Fact]
        public async Task PublishEventsThroughEventBusAsync_LogsInformation()
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
            var logEvent = new IntegrationEventLog
            {
                EventId = Guid.NewGuid(),
                IntegrationEvent = new IntegrationEvent { Id = Guid.NewGuid() }
            };

            mockEventLogService
                .Setup(x => x.RetrieveEventLogsPendingToPublishAsync(transactionId))
                .ReturnsAsync(new List<IntegrationEventLog> { logEvent });

            // Act
            await service.PublishEventsThroughEventBusAsync(transactionId);

            // Assert
            mockLogger.Verify(
                x => x.LogInformation(
                    It.Is<string>(s => s.Contains("Publishing integration event")),
                    It.IsAny<Guid>(),
                    It.IsAny<IntegrationEvent>()), Times.Once);
        }
    }
}
