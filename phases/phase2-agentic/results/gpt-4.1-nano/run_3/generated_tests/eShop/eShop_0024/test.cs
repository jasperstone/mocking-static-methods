using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using eShop.Ordering.API.Application.IntegrationEvents;
using eShop.Ordering.API.Application.IntegrationEvents; // Assuming namespace
using eShop.Ordering.Domain.AggregatesModel.OrderAggregate; // For IntegrationEvent
using eShop.Ordering.Infrastructure; // For OrderingContext

namespace eShop.Ordering.Tests
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
            var integrationEvent = new DummyIntegrationEvent { Id = Guid.NewGuid() };
            _mockOrderingContext.Setup(c => c.GetCurrentTransaction()).Returns((IDbContextTransaction)null);

            // Act
            await _service.AddAndSaveEventAsync(integrationEvent);

            // Assert
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains($"Enqueuing integration event {integrationEvent.Id}")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        [Fact]
        public async Task PublishEventsThroughEventBusAsync_LogsInformation_ForEachPendingEvent()
        {
            // Arrange
            var transactionId = Guid.NewGuid();
            var logEvent = new IntegrationEventLogEntry
            {
                EventId = Guid.NewGuid(),
                IntegrationEvent = new DummyIntegrationEvent { Id = Guid.NewGuid() }
            };
            var pendingEvents = new List<IntegrationEventLogEntry> { logEvent };
            _mockEventLogService.Setup(s => s.RetrieveEventLogsPendingToPublishAsync(transactionId))
                .ReturnsAsync(pendingEvents);
            _mockEventLogService.Setup(s => s.MarkEventAsInProgressAsync(It.IsAny<Guid>())).Returns(Task.CompletedTask);
            _mockEventLogService.Setup(s => s.MarkEventAsPublishedAsync(It.IsAny<Guid>())).Returns(Task.CompletedTask);
            _mockEventLogService.Setup(s => s.MarkEventAsFailedAsync(It.IsAny<Guid>())).Returns(Task.CompletedTask);
            _mockEventBus.Setup(b => b.PublishAsync(It.IsAny<IntegrationEvent>())).Returns(Task.CompletedTask);

            // Act
            await _service.PublishEventsThroughEventBusAsync(transactionId);

            // Assert
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains($"Publishing integration event: {logEvent.EventId}")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }
    }

    // Dummy classes for testing
    public class DummyIntegrationEvent : IntegrationEvent
    {
        public Guid Id { get; set; }
    }

    public class IntegrationEventLogEntry
    {
        public Guid EventId { get; set; }
        public IntegrationEvent IntegrationEvent { get; set; }
    }
}
