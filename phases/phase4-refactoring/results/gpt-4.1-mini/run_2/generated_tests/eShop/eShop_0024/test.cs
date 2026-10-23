using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using eShop.Ordering.API.Application.IntegrationEvents;
using eShop.EventBus.Abstractions;
using eShop.EventBus.Events;
using eShop.IntegrationEventLogEF.Services;
using eShop.Ordering.Infrastructure;

namespace eShop.Ordering.API.Tests.Application.IntegrationEvents
{
    public class OrderingIntegrationEventServiceTests
    {
        [Fact]
        public async Task PublishEventsThroughEventBusAsync_LogsInformationForEachEvent()
        {
            // Arrange
            var transactionId = Guid.NewGuid();

            var mockEventBus = new Mock<IEventBus>();
            var mockOrderingContext = new Mock<OrderingContext>();
            var mockEventLogService = new Mock<IIntegrationEventLogService>();
            var mockLogger = new Mock<ILogger<OrderingIntegrationEventService>>();

            var integrationEvent = new IntegrationEvent();
            var logEvent = new IntegrationEventLogEntry
            {
                EventId = Guid.NewGuid(),
                IntegrationEvent = integrationEvent
            };

            mockEventLogService.Setup(s => s.RetrieveEventLogsPendingToPublishAsync(transactionId))
                .ReturnsAsync(new List<IntegrationEventLogEntry> { logEvent });

            mockEventLogService.Setup(s => s.MarkEventAsInProgressAsync(logEvent.EventId))
                .Returns(Task.CompletedTask);
            mockEventBus.Setup(b => b.PublishAsync(integrationEvent))
                .Returns(Task.CompletedTask);
            mockEventLogService.Setup(s => s.MarkEventAsPublishedAsync(logEvent.EventId))
                .Returns(Task.CompletedTask);

            var service = new OrderingIntegrationEventService(
                mockEventBus.Object,
                mockOrderingContext.Object,
                mockEventLogService.Object,
                mockLogger.Object);

            // Act
            await service.PublishEventsThroughEventBusAsync(transactionId);

            // Assert
            mockLogger.Verify(
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
            var mockEventBus = new Mock<IEventBus>();
            var mockOrderingContext = new Mock<OrderingContext>();
            var mockEventLogService = new Mock<IIntegrationEventLogService>();
            var mockLogger = new Mock<ILogger<OrderingIntegrationEventService>>();

            var integrationEvent = new IntegrationEvent();

            var mockTransaction = new Mock<IDbContextTransaction>();
            mockOrderingContext.Setup(c => c.GetCurrentTransaction()).Returns(mockTransaction.Object);

            mockEventLogService.Setup(s => s.SaveEventAsync(integrationEvent, mockTransaction.Object))
                .Returns(Task.CompletedTask);

            var service = new OrderingIntegrationEventService(
                mockEventBus.Object,
                mockOrderingContext.Object,
                mockEventLogService.Object,
                mockLogger.Object);

            // Act
            await service.AddAndSaveEventAsync(integrationEvent);

            // Assert
            mockLogger.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Enqueuing integration event")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);

            mockEventLogService.Verify(s => s.SaveEventAsync(integrationEvent, mockTransaction.Object), Times.Once);
        }
    }

    // Minimal stub for IntegrationEventLogEntry to allow compilation of tests
    public class IntegrationEventLogEntry
    {
        public Guid EventId { get; set; }
        public IntegrationEvent IntegrationEvent { get; set; }
    }
}
