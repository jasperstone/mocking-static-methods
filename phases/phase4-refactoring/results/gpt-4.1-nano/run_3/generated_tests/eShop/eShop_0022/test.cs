using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using eShop.Ordering.API.Application.IntegrationEvents;

namespace eShop.Ordering.Tests
{
    public class OrderingIntegrationEventServiceTests
    {
        private readonly Mock<IEventBus> _eventBusMock;
        private readonly Mock<IIntegrationEventLogService> _eventLogServiceMock;
        private readonly Mock<ILogger<OrderingIntegrationEventService>> _loggerMock;
        private readonly Mock<OrderingContext> _orderingContextMock;

        public OrderingIntegrationEventServiceTests()
        {
            _eventBusMock = new Mock<IEventBus>();
            _eventLogServiceMock = new Mock<IIntegrationEventLogService>();
            _loggerMock = new Mock<ILogger<OrderingIntegrationEventService>>();
            _orderingContextMock = new Mock<OrderingContext>();
        }

        [Fact]
        public async Task PublishEventsThroughEventBusAsync_ShouldLogInformation()
        {
            // Arrange
            var transactionId = Guid.NewGuid();
            var logEvent = new Mock<IntegrationEventLogEntry>();
            var eventId = Guid.NewGuid();
            var integrationEvent = new Mock<IntegrationEvent>();
            var logEvents = new List<IntegrationEventLogEntry> { logEvent.Object };

            logEvent.Setup(e => e.EventId).Returns(eventId);
            logEvent.Setup(e => e.IntegrationEvent).Returns(integrationEvent.Object);

            _eventLogServiceMock.Setup(s => s.RetrieveEventLogsPendingToPublishAsync(transactionId))
                .ReturnsAsync(logEvents);
            _eventLogServiceMock.Setup(s => s.MarkEventAsInProgressAsync(It.IsAny<Guid>()))
                .Returns(Task.CompletedTask);
            _eventLogServiceMock.Setup(s => s.MarkEventAsPublishedAsync(It.IsAny<Guid>()))
                .Returns(Task.CompletedTask);
            _eventBusMock.Setup(b => b.PublishAsync(It.IsAny<IntegrationEvent>()))
                .Returns(Task.CompletedTask);

            var service = new OrderingIntegrationEventService(
                _eventBusMock.Object,
                _orderingContextMock.Object,
                _eventLogServiceMock.Object,
                _loggerMock.Object);

            // Act
            await service.PublishEventsThroughEventBusAsync(transactionId);

            // Assert
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains($"Publishing integration event: {eventId}")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }
    }
}
