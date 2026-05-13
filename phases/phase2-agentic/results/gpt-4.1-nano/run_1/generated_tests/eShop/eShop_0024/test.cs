using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using eShop.Ordering.API.Application.IntegrationEvents;
using eShop.Ordering.API.Application.IntegrationEvents;
using eShop.Ordering.Infrastructure;
using eShop.SharedKernel.IntegrationEvents;

namespace eShop.Tests
{
    public class OrderingIntegrationEventServiceTests
    {
        private readonly Mock<IEventBus> _mockEventBus;
        private readonly Mock<IIntegrationEventLogService> _mockEventLogService;
        private readonly Mock<ILogger<OrderingIntegrationEventService>> _mockLogger;
        private readonly Mock<OrderingContext> _mockOrderingContext;
        private readonly OrderingIntegrationEventService _service;

        public OrderingIntegrationEventServiceTests()
        {
            _mockEventBus = new Mock<IEventBus>();
            _mockEventLogService = new Mock<IIntegrationEventLogService>();
            _mockLogger = new Mock<ILogger<OrderingIntegrationEventService>>();
            _mockOrderingContext = new Mock<OrderingContext>();
            _service = new OrderingIntegrationEventService(
                _mockEventBus.Object,
                _mockOrderingContext.Object,
                _mockEventLogService.Object,
                _mockLogger.Object);
        }

        [Fact]
        public async Task AddAndSaveEventAsync_LogsInformation()
        {
            // Arrange
            var integrationEvent = new TestIntegrationEvent { Id = Guid.NewGuid() };
            var mockTransaction = new Mock<IDbContextTransaction>();
            _mockOrderingContext.Setup(c => c.GetCurrentTransaction()).Returns(mockTransaction.Object);

            // Act
            await _service.AddAndSaveEventAsync(integrationEvent);

            // Assert
            _mockLogger.Verify(
                x => x.LogInformation(
                    It.Is<string>(s => s.Contains("Enqueuing")),
                    integrationEvent.Id,
                    It.IsAny<IntegrationEvent>()),
                Times.Once);
            _mockEventLogService.Verify(s => s.SaveEventAsync(integrationEvent, mockTransaction.Object), Times.Once);
        }

        [Fact]
        public async Task PublishEventsThroughEventBusAsync_LogsInformation_ForEachPendingEvent()
        {
            // Arrange
            var transactionId = Guid.NewGuid();
            var logEvent1 = new EventLogEntry
            {
                EventId = Guid.NewGuid(),
                IntegrationEvent = new TestIntegrationEvent { Id = Guid.NewGuid() }
            };
            var logEvent2 = new EventLogEntry
            {
                EventId = Guid.NewGuid(),
                IntegrationEvent = new TestIntegrationEvent { Id = Guid.NewGuid() }
            };
            var pendingEvents = new List<EventLogEntry> { logEvent1, logEvent2 };

            _mockEventLogService.Setup(s => s.RetrieveEventLogsPendingToPublishAsync(transactionId))
                .ReturnsAsync(pendingEvents);

            // Act
            await _service.PublishEventsThroughEventBusAsync(transactionId);

            // Assert
            _mockLogger.Verify(
                x => x.LogInformation(
                    It.Is<string>(s => s.Contains("Publishing integration event")),
                    It.IsAny<Guid>(), It.IsAny<IntegrationEvent>()),
                Times.Exactly(2));
        }
    }

    // Dummy classes for testing
    public class TestIntegrationEvent : IntegrationEvent
    {
        public override Guid Id { get; set; } = Guid.NewGuid();
    }

    public class EventLogEntry
    {
        public Guid EventId { get; set; }
        public IntegrationEvent IntegrationEvent { get; set; }
    }
}
