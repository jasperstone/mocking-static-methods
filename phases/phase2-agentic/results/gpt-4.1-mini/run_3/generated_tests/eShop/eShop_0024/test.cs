using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using eShop.Ordering.API.Application.IntegrationEvents;

namespace eShop.Ordering.API.Tests.Application.IntegrationEvents
{
    public class OrderingIntegrationEventServiceTests
    {
        private readonly Mock<IEventBus> _eventBusMock;
        private readonly Mock<OrderingContext> _orderingContextMock;
        private readonly Mock<IIntegrationEventLogService> _eventLogServiceMock;
        private readonly Mock<ILogger<OrderingIntegrationEventService>> _loggerMock;
        private readonly OrderingIntegrationEventService _service;

        public OrderingIntegrationEventServiceTests()
        {
            _eventBusMock = new Mock<IEventBus>();
            _orderingContextMock = new Mock<OrderingContext>();
            _eventLogServiceMock = new Mock<IIntegrationEventLogService>();
            _loggerMock = new Mock<ILogger<OrderingIntegrationEventService>>();

            _service = new OrderingIntegrationEventService(
                _eventBusMock.Object,
                _orderingContextMock.Object,
                _eventLogServiceMock.Object,
                _loggerMock.Object);
        }

        [Fact]
        public async Task PublishEventsThroughEventBusAsync_LogsInformationForEachEvent()
        {
            // Arrange
            var transactionId = Guid.NewGuid();
            var integrationEvent = new IntegrationEvent(Guid.NewGuid());
            var logEvent = new IntegrationEventLogEntry
            {
                EventId = integrationEvent.Id,
                IntegrationEvent = integrationEvent
            };
            var pendingEvents = new List<IntegrationEventLogEntry> { logEvent };

            _eventLogServiceMock
                .Setup(s => s.RetrieveEventLogsPendingToPublishAsync(transactionId))
                .ReturnsAsync(pendingEvents);

            _eventLogServiceMock
                .Setup(s => s.MarkEventAsInProgressAsync(logEvent.EventId))
                .Returns(Task.CompletedTask);

            _eventBusMock
                .Setup(b => b.PublishAsync(logEvent.IntegrationEvent))
                .Returns(Task.CompletedTask);

            _eventLogServiceMock
                .Setup(s => s.MarkEventAsPublishedAsync(logEvent.EventId))
                .Returns(Task.CompletedTask);

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

        [Fact]
        public async Task AddAndSaveEventAsync_LogsInformationAndSavesEvent()
        {
            // Arrange
            var integrationEvent = new IntegrationEvent(Guid.NewGuid());
            var transaction = new object();
            _orderingContextMock.Setup(c => c.GetCurrentTransaction()).Returns(transaction);
            _eventLogServiceMock.Setup(s => s.SaveEventAsync(integrationEvent, transaction)).Returns(Task.CompletedTask);

            // Act
            await _service.AddAndSaveEventAsync(integrationEvent);

            // Assert
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Enqueuing integration event")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);

            _eventLogServiceMock.Verify(s => s.SaveEventAsync(integrationEvent, transaction), Times.Once);
        }
    }

    // Minimal stubs for dependencies and models to make the test compile
    public class IntegrationEvent
    {
        public Guid Id { get; }
        public IntegrationEvent(Guid id) => Id = id;
    }

    public class IntegrationEventLogEntry
    {
        public Guid EventId { get; set; }
        public IntegrationEvent IntegrationEvent { get; set; }
    }

    public interface IEventBus
    {
        Task PublishAsync(IntegrationEvent evt);
    }

    public interface IIntegrationEventLogService
    {
        Task<List<IntegrationEventLogEntry>> RetrieveEventLogsPendingToPublishAsync(Guid transactionId);
        Task MarkEventAsInProgressAsync(Guid eventId);
        Task MarkEventAsPublishedAsync(Guid eventId);
        Task MarkEventAsFailedAsync(Guid eventId);
        Task SaveEventAsync(IntegrationEvent evt, object transaction);
    }

    public class OrderingContext
    {
        public virtual object GetCurrentTransaction() => null;
    }
}
