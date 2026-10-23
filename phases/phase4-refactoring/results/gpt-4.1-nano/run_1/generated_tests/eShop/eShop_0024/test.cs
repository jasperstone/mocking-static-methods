using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using eShop.Ordering.API.Application.IntegrationEvents;

namespace eShop.Tests
{
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
        public async Task AddAndSaveEventAsync_LogsInformationAndCallsSave()
        {
            // Arrange
            var evt = new DummyIntegrationEvent { Id = Guid.NewGuid() };
            _orderingContextMock.Setup(c => c.GetCurrentTransaction()).Returns((IDisposable)null);

            // Act
            await _service.AddAndSaveEventAsync(evt);

            // Assert
            _loggerMock.Verify(
                x => x.LogInformation(
                    It.Is<string>(s => s.Contains("Enqueuing integration event")),
                    evt.Id,
                    evt),
                Times.Once);
            _eventLogServiceMock.Verify(s => s.SaveEventAsync(evt, null), Times.Once);
        }

        [Fact]
        public async Task PublishEventsThroughEventBusAsync_LogsInformationAndHandlesSuccess()
        {
            // Arrange
            var transactionId = Guid.NewGuid();
            var logEvent = new LogEvent
            {
                EventId = Guid.NewGuid(),
                IntegrationEvent = new DummyIntegrationEvent { Id = Guid.NewGuid() }
            };
            var pendingEvents = new List<LogEvent> { logEvent };
            _eventLogServiceMock.Setup(s => s.RetrieveEventLogsPendingToPublishAsync(transactionId))
                .ReturnsAsync(pendingEvents);
            _eventLogServiceMock.Setup(s => s.MarkEventAsInProgressAsync(logEvent.EventId))
                .Returns(Task.CompletedTask);
            _eventLogServiceMock.Setup(s => s.MarkEventAsPublishedAsync(logEvent.EventId))
                .Returns(Task.CompletedTask);
            _eventBusMock.Setup(b => b.PublishAsync(logEvent.IntegrationEvent))
                .Returns(Task.CompletedTask);

            // Act
            await _service.PublishEventsThroughEventBusAsync(transactionId);

            // Assert
            _loggerMock.Verify(
                x => x.LogInformation(
                    It.Is<string>(s => s.Contains("Publishing integration event")),
                    logEvent.EventId,
                    logEvent.IntegrationEvent),
                Times.Once);
            _eventLogServiceMock.Verify(s => s.MarkEventAsInProgressAsync(logEvent.EventId), Times.Once);
            _eventLogServiceMock.Verify(s => s.MarkEventAsPublishedAsync(logEvent.EventId), Times.Once);
            _eventBusMock.Verify(b => b.PublishAsync(logEvent.IntegrationEvent), Times.Once);
        }

        [Fact]
        public async Task PublishEventsThroughEventBusAsync_LogsErrorAndMarksFailedOnException()
        {
            // Arrange
            var transactionId = Guid.NewGuid();
            var logEvent = new LogEvent
            {
                EventId = Guid.NewGuid(),
                IntegrationEvent = new DummyIntegrationEvent { Id = Guid.NewGuid() }
            };
            var pendingEvents = new List<LogEvent> { logEvent };
            _eventLogServiceMock.Setup(s => s.RetrieveEventLogsPendingToPublishAsync(transactionId))
                .ReturnsAsync(pendingEvents);
            _eventLogServiceMock.Setup(s => s.MarkEventAsInProgressAsync(logEvent.EventId))
                .Returns(Task.CompletedTask);
            _eventLogServiceMock.Setup(s => s.MarkEventAsFailedAsync(logEvent.EventId))
                .Returns(Task.CompletedTask);
            _eventBusMock.Setup(b => b.PublishAsync(logEvent.IntegrationEvent))
                .ThrowsAsync(new Exception("Publish failed"));

            // Act
            await _service.PublishEventsThroughEventBusAsync(transactionId);

            // Assert
            _loggerMock.Verify(
                x => x.LogError(
                    It.Is<Exception>(ex => ex.Message.Contains("Publish failed")),
                    "Error publishing integration event: {IntegrationEventId}",
                    logEvent.EventId),
                Times.Once);
            _eventLogServiceMock.Verify(s => s.MarkEventAsFailedAsync(logEvent.EventId), Times.Once);
        }
    }

    // Dummy classes for testing
    public class DummyIntegrationEvent : IntegrationEvent
    {
        public override Guid Id { get; set; } = Guid.NewGuid();
    }

    public class LogEvent
    {
        public Guid EventId { get; set; }
        public IntegrationEvent IntegrationEvent { get; set; }
    }
}
