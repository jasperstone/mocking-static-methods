using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using eShop.Ordering.API.Application.IntegrationEvents;

namespace eShop.Tests
{
    // Minimal stub for IntegrationEvent
    public class IntegrationEvent
    {
        public Guid Id { get; set; } = Guid.NewGuid();
    }

    // Minimal stub for log entry
    public class IntegrationEventLogEntry
    {
        public Guid EventId { get; set; }
        public IntegrationEvent IntegrationEvent { get; set; }
    }

    public class OrderingIntegrationEventServiceTests
    {
        private readonly Mock<IEventBus> _eventBusMock;
        private readonly Mock<IIntegrationEventLogService> _eventLogServiceMock;
        private readonly Mock<ILogger<OrderingIntegrationEventService>> _loggerMock;
        private readonly Mock<OrderingContext> _orderingContextMock;
        private readonly OrderingIntegrationEventService _service;

        public OrderingIntegrationEventServiceTests()
        {
            _eventBusMock = new Mock<IEventBus>();
            _eventLogServiceMock = new Mock<IIntegrationEventLogService>();
            _loggerMock = new Mock<ILogger<OrderingIntegrationEventService>>();
            _orderingContextMock = new Mock<OrderingContext>();
            _service = new OrderingIntegrationEventService(
                _eventBusMock.Object,
                _orderingContextMock.Object,
                _eventLogServiceMock.Object,
                _loggerMock.Object);
        }

        [Fact]
        public async Task PublishEventsThroughEventBusAsync_LogsInformation()
        {
            // Arrange
            var transactionId = Guid.NewGuid();
            var logEvents = new List<IntegrationEventLogEntry>
            {
                new IntegrationEventLogEntry
                {
                    EventId = Guid.NewGuid(),
                    IntegrationEvent = new IntegrationEvent { Id = Guid.NewGuid() }
                }
            };
            _eventLogServiceMock.Setup(s => s.RetrieveEventLogsPendingToPublishAsync(transactionId))
                .ReturnsAsync(logEvents);

            // Act
            await _service.PublishEventsThroughEventBusAsync(transactionId);

            // Assert
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Publishing integration event")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }
    }
}
