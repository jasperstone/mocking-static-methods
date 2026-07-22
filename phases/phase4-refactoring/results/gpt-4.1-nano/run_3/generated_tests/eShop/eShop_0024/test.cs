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
            _orderingContextMock.Setup(c => c.GetCurrentTransaction()).Returns((object)null);

            // Act
            await _service.AddAndSaveEventAsync(evt);

            // Assert
            _loggerMock.Verify(
                x => x.LogInformation(
                    It.Is<string>(s => s.Contains("Enqueuing integration event")),
                    evt.Id,
                    It.IsAny<object>()),
                Times.Once);
            _eventLogServiceMock.Verify(s => s.SaveEventAsync(evt, null), Times.Once);
        }

        [Fact]
        public async Task PublishEventsThroughEventBusAsync_LogsInformationAndHandlesEvents()
        {
            // Arrange
            var transactionId = Guid.NewGuid();
            var logEvent = new
            {
                EventId = Guid.NewGuid(),
                IntegrationEvent = new { Name = "TestEvent" }
            };
            var pendingEvents = new List<dynamic> { logEvent };
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
    }
}
