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
            var transactionMock = new Mock<IDbContextTransaction>();
            _orderingContextMock.Setup(c => c.GetCurrentTransaction()).Returns(transactionMock.Object);

            // Act
            await _service.AddAndSaveEventAsync(evt);

            // Assert
            _loggerMock.Verify(
                x => x.LogInformation(
                    "Enqueuing integration event {IntegrationEventId} to repository ({@IntegrationEvent})",
                    evt.Id,
                    evt),
                Times.Once);

            _eventLogServiceMock.Verify(
                s => s.SaveEventAsync(evt, transactionMock.Object),
                Times.Once);
        }

        [Fact]
        public async Task PublishEventsThroughEventBusAsync_LogsInformationAndHandlesEvents()
        {
            // Arrange
            var transactionId = Guid.NewGuid();
            var logEvent = new Mock<IntegrationEventLogEntry>();
            var eventId = Guid.NewGuid();
            var integrationEvent = new Mock<IntegrationEvent>();
            logEvent.SetupGet(e => e.EventId).Returns(eventId);
            logEvent.SetupGet(e => e.IntegrationEvent).Returns(integrationEvent.Object);
            var pendingEvents = new List<IntegrationEventLogEntry> { logEvent.Object };

            _eventLogServiceMock.Setup(s => s.RetrieveEventLogsPendingToPublishAsync(transactionId))
                .ReturnsAsync(pendingEvents);
            _eventLogServiceMock.Setup(s => s.MarkEventAsInProgressAsync(eventId))
                .Returns(Task.CompletedTask);
            _eventLogServiceMock.Setup(s => s.MarkEventAsPublishedAsync(eventId))
                .Returns(Task.CompletedTask);
            _eventLogServiceMock.Setup(s => s.MarkEventAsFailedAsync(It.IsAny<Guid>()))
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
            _eventBusMock.Verify(s => s.PublishAsync(integrationEvent.Object), Times.Once);
            _eventLogServiceMock.Verify(s => s.MarkEventAsPublishedAsync(eventId), Times.Once);
        }
    }
}
