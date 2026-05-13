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
            var evt = new Mock<IntegrationEvent>();
            var transactionMock = new Mock<IDbContextTransaction>();
            _orderingContextMock.Setup(c => c.GetCurrentTransaction()).Returns(transactionMock.Object);
            var evtId = Guid.NewGuid();
            evt.Setup(e => e.Id).Returns(evtId);

            // Act
            await _service.AddAndSaveEventAsync(evt.Object);

            // Assert
            _loggerMock.Verify(
                x => x.LogInformation(
                    "Enqueuing integration event {IntegrationEventId} to repository ({@IntegrationEvent})",
                    evtId,
                    evt.Object),
                Times.Once);
            _eventLogServiceMock.Verify(s => s.SaveEventAsync(evt.Object, transactionMock.Object), Times.Once);
        }

        [Fact]
        public async Task PublishEventsThroughEventBusAsync_LogsInformationAndHandlesSuccess()
        {
            // Arrange
            var transactionId = Guid.NewGuid();
            var logEvent = new Mock<IntegrationEventLogEntry>();
            var eventId = Guid.NewGuid();
            var integrationEvent = new Mock<IntegrationEvent>();
            logEvent.Setup(e => e.EventId).Returns(eventId);
            logEvent.Setup(e => e.IntegrationEvent).Returns(integrationEvent.Object);
            var pendingEvents = new List<IntegrationEventLogEntry> { logEvent.Object };

            _eventLogServiceMock.Setup(s => s.RetrieveEventLogsPendingToPublishAsync(transactionId))
                .ReturnsAsync(pendingEvents);
            _eventLogServiceMock.Setup(s => s.MarkEventAsInProgressAsync(eventId))
                .Returns(Task.CompletedTask);
            _eventLogServiceMock.Setup(s => s.MarkEventAsPublishedAsync(eventId))
                .Returns(Task.CompletedTask);
            _eventBusMock.Setup(b => b.PublishAsync(integrationEvent.Object))
                .Returns(Task.CompletedTask);

            // Act
            await _service.PublishEventsThroughEventBusAsync(transactionId);

            // Assert
            _loggerMock.Verify(
                x => x.LogInformation(
                    "Publishing integration event: {IntegrationEventId} - ({@IntegrationEvent})",
                    eventId,
                    integrationEvent.Object),
                Times.Once);
            _eventLogServiceMock.Verify(s => s.MarkEventAsInProgressAsync(eventId), Times.Once);
            _eventBusMock.Verify(b => b.PublishAsync(integrationEvent.Object), Times.Once);
            _eventLogServiceMock.Verify(s => s.MarkEventAsPublishedAsync(eventId), Times.Once);
        }

        [Fact]
        public async Task PublishEventsThroughEventBusAsync_LogsErrorAndMarksFailedOnException()
        {
            // Arrange
            var transactionId = Guid.NewGuid();
            var logEvent = new Mock<IntegrationEventLogEntry>();
            var eventId = Guid.NewGuid();
            var integrationEvent = new Mock<IntegrationEvent>();
            logEvent.Setup(e => e.EventId).Returns(eventId);
            logEvent.Setup(e => e.IntegrationEvent).Returns(integrationEvent.Object);
            var pendingEvents = new List<IntegrationEventLogEntry> { logEvent.Object };

            _eventLogServiceMock.Setup(s => s.RetrieveEventLogsPendingToPublishAsync(transactionId))
                .ReturnsAsync(pendingEvents);
            _eventLogServiceMock.Setup(s => s.MarkEventAsInProgressAsync(eventId))
                .Returns(Task.CompletedTask);
            _eventLogServiceMock.Setup(s => s.MarkEventAsPublishedAsync(eventId))
                .Returns(Task.CompletedTask);
            _eventBusMock.Setup(b => b.PublishAsync(integrationEvent.Object))
                .ThrowsAsync(new Exception("Publish failed"));

            // Act
            await _service.PublishEventsThroughEventBusAsync(transactionId);

            // Assert
            _loggerMock.Verify(
                x => x.LogError(It.IsAny<Exception>(), "Error publishing integration event: {IntegrationEventId}", eventId),
                Times.Once);
            _eventLogServiceMock.Verify(s => s.MarkEventAsFailedAsync(eventId), Times.Once);
        }
    }
}
