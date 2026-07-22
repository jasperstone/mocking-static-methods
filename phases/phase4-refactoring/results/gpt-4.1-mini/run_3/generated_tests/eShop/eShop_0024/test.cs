using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using eShop.EventBus.Abstractions;
using eShop.EventBus.Events;
using eShop.IntegrationEventLogEF.Services;
using eShop.Ordering.API.Application.IntegrationEvents;
using eShop.Ordering.API.Infrastructure;

namespace eShop.Ordering.Tests.Application.IntegrationEvents
{
    public class OrderingIntegrationEventServiceTests
    {
        private readonly Mock<IEventBus> _eventBusMock = new();
        private readonly Mock<IIntegrationEventLogService> _eventLogServiceMock = new();
        private readonly Mock<OrderingContext> _orderingContextMock = new();
        private readonly Mock<ILogger<OrderingIntegrationEventService>> _loggerMock = new();

        private OrderingIntegrationEventService CreateService()
        {
            return new OrderingIntegrationEventService(
                _eventBusMock.Object,
                _orderingContextMock.Object,
                _eventLogServiceMock.Object,
                _loggerMock.Object);
        }

        [Fact]
        public async Task PublishEventsThroughEventBusAsync_LogsInformationForEachPendingEvent()
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

            _eventLogServiceMock.Setup(s => s.RetrieveEventLogsPendingToPublishAsync(transactionId))
                .ReturnsAsync(pendingEvents);
            _eventLogServiceMock.Setup(s => s.MarkEventAsInProgressAsync(logEvent.EventId))
                .Returns(Task.CompletedTask);
            _eventBusMock.Setup(b => b.PublishAsync(integrationEvent))
                .Returns(Task.CompletedTask);
            _eventLogServiceMock.Setup(s => s.MarkEventAsPublishedAsync(logEvent.EventId))
                .Returns(Task.CompletedTask);

            var service = CreateService();

            // Act
            await service.PublishEventsThroughEventBusAsync(transactionId);

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
            var transactionMock = new Mock<IDbContextTransaction>();
            _orderingContextMock.Setup(c => c.GetCurrentTransaction()).Returns(transactionMock.Object);
            _eventLogServiceMock.Setup(s => s.SaveEventAsync(integrationEvent, transactionMock.Object))
                .Returns(Task.CompletedTask);

            var service = CreateService();

            // Act
            await service.AddAndSaveEventAsync(integrationEvent);

            // Assert
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Enqueuing integration event")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);

            _eventLogServiceMock.Verify(s => s.SaveEventAsync(integrationEvent, transactionMock.Object), Times.Once);
        }

        // Helper class to simulate IntegrationEventLogEntry
        public class IntegrationEventLogEntry
        {
            public Guid EventId { get; set; }
            public IntegrationEvent IntegrationEvent { get; set; }
        }
    }
}
