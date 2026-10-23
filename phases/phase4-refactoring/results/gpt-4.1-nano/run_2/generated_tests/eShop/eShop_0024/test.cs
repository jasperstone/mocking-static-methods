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
            var evt = new IntegrationEvent { Id = Guid.NewGuid() };
            _orderingContextMock.Setup(c => c.GetCurrentTransaction()).Returns((IDbContextTransaction)null);

            // Act
            await _service.AddAndSaveEventAsync(evt);

            // Assert
            _loggerMock.Verify(
                x => x.LogInformation(
                    It.Is<string>(s => s.Contains("Enqueuing")),
                    evt.Id,
                    It.IsAny<object>()),
                Times.Once);
            _eventLogServiceMock.Verify(s => s.SaveEventAsync(evt, null), Times.Once);
        }

        [Fact]
        public async Task PublishEventsThroughEventBusAsync_LogsInformationAndHandlesSuccess()
        {
            // Arrange
            var transactionId = Guid.NewGuid();
            var logEvent = new Mock<IntegrationEventLogEntry>();
            logEvent.SetupGet(e => e.EventId).Returns(Guid.NewGuid());
            logEvent.SetupGet(e => e.IntegrationEvent).Returns(new IntegrationEvent { Id = Guid.NewGuid() });
            var logs = new List<IntegrationEventLogEntry> { logEvent.Object };

            _eventLogServiceMock.Setup(s => s.RetrieveEventLogsPendingToPublishAsync(transactionId))
                .ReturnsAsync(logs);
            _eventLogServiceMock.Setup(s => s.MarkEventAsInProgressAsync(It.IsAny<Guid>()))
                .Returns(Task.CompletedTask);
            _eventLogServiceMock.Setup(s => s.MarkEventAsPublishedAsync(It.IsAny<Guid>()))
                .Returns(Task.CompletedTask);
            _eventBusMock.Setup(b => b.PublishAsync(It.IsAny<IntegrationEvent>()))
                .Returns(Task.CompletedTask);

            // Act
            await _service.PublishEventsThroughEventBusAsync(transactionId);

            // Assert
            _loggerMock.Verify(
                x => x.LogInformation(
                    It.Is<string>(s => s.Contains("Publishing integration event")),
                    It.IsAny<Guid>(), It.IsAny<IntegrationEvent>()),
                Times.Once);
            _eventLogServiceMock.Verify(s => s.MarkEventAsInProgressAsync(It.IsAny<Guid>()), Times.Once);
            _eventLogServiceMock.Verify(s => s.MarkEventAsPublishedAsync(It.IsAny<Guid>()), Times.Once);
        }

        [Fact]
        public async Task PublishEventsThroughEventBusAsync_LogsErrorAndMarksFailedOnException()
        {
            // Arrange
            var transactionId = Guid.NewGuid();
            var logEvent = new Mock<IntegrationEventLogEntry>();
            var eventId = Guid.NewGuid();
            logEvent.SetupGet(e => e.EventId).Returns(eventId);
            logEvent.SetupGet(e => e.IntegrationEvent).Returns(new IntegrationEvent { Id = Guid.NewGuid() });
            var logs = new List<IntegrationEventLogEntry> { logEvent.Object };

            _eventLogServiceMock.Setup(s => s.RetrieveEventLogsPendingToPublishAsync(transactionId))
                .ReturnsAsync(logs);
            _eventLogServiceMock.Setup(s => s.MarkEventAsInProgressAsync(It.IsAny<Guid>()))
                .Returns(Task.CompletedTask);
            _eventLogServiceMock.Setup(s => s.MarkEventAsFailedAsync(It.IsAny<Guid>()))
                .Returns(Task.CompletedTask);
            _eventBusMock.Setup(b => b.PublishAsync(It.IsAny<IntegrationEvent>()))
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
