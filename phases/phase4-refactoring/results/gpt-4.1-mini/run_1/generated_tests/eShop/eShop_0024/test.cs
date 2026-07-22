using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using eShop.EventBus.Abstractions;
using eShop.EventBus.Events;
using eShop.IntegrationEventLogEF;
using eShop.IntegrationEventLogEF.Services;
using eShop.Ordering.API.Application.IntegrationEvents;
using eShop.Ordering.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace eShop.Ordering.API.Tests.Application.IntegrationEvents
{
    public class OrderingIntegrationEventServiceTests
    {
        private readonly Mock<IEventBus> _eventBusMock;
        private readonly Mock<IIntegrationEventLogService> _eventLogServiceMock;
        private readonly Mock<OrderingContext> _orderingContextMock;
        private readonly Mock<ILogger<OrderingIntegrationEventService>> _loggerMock;

        public OrderingIntegrationEventServiceTests()
        {
            _eventBusMock = new Mock<IEventBus>();
            _eventLogServiceMock = new Mock<IIntegrationEventLogService>();
            _orderingContextMock = new Mock<OrderingContext>();
            _loggerMock = new Mock<ILogger<OrderingIntegrationEventService>>();
        }

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
            var integrationEvent = new IntegrationEvent();
            var logEvent = new IntegrationEventLogEntry(integrationEvent, transactionId);
            // Set the IntegrationEvent property explicitly for test
            logEvent.DeserializeJsonContent(typeof(IntegrationEvent));

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
        public async Task AddAndSaveEventAsync_LogsInformation()
        {
            // Arrange
            var integrationEvent = new IntegrationEvent();
            var dbContextTransactionMock = new Mock<IDbContextTransaction>();
            _orderingContextMock.Setup(c => c.GetCurrentTransaction()).Returns(dbContextTransactionMock.Object);
            _eventLogServiceMock.Setup(s => s.SaveEventAsync(integrationEvent, dbContextTransactionMock.Object))
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
        }
    }
}
