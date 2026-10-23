using System;
using System.Collections.Generic;
using System.Threading.Tasks;
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
        [Fact]
        public async Task PublishEventsThroughEventBusAsync_LogsInformationForEachEvent()
        {
            // Arrange
            var transactionId = Guid.NewGuid();

            var mockEventBus = new Mock<IEventBus>();
            var mockOrderingContext = new Mock<OrderingContext>(new DbContextOptions<OrderingContext>());
            var mockIntegrationEventLogService = new Mock<IIntegrationEventLogService>();
            var mockLogger = new Mock<ILogger<OrderingIntegrationEventService>>();

            var integrationEvent = new TestIntegrationEvent(Guid.NewGuid());
            var logEvent = new IntegrationEventLogEntry
            {
                EventId = integrationEvent.Id,
                IntegrationEvent = integrationEvent
            };

            mockIntegrationEventLogService
                .Setup(s => s.RetrieveEventLogsPendingToPublishAsync(transactionId))
                .ReturnsAsync(new List<IntegrationEventLogEntry> { logEvent });

            mockIntegrationEventLogService
                .Setup(s => s.MarkEventAsInProgressAsync(logEvent.EventId))
                .Returns(Task.CompletedTask);

            mockEventBus
                .Setup(b => b.PublishAsync(integrationEvent))
                .Returns(Task.CompletedTask);

            mockIntegrationEventLogService
                .Setup(s => s.MarkEventAsPublishedAsync(logEvent.EventId))
                .Returns(Task.CompletedTask);

            var service = new OrderingIntegrationEventService(
                mockEventBus.Object,
                mockOrderingContext.Object,
                mockIntegrationEventLogService.Object,
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
            var mockOrderingContext = new Mock<OrderingContext>(new DbContextOptions<OrderingContext>());
            var mockIntegrationEventLogService = new Mock<IIntegrationEventLogService>();
            var mockLogger = new Mock<ILogger<OrderingIntegrationEventService>>();

            var integrationEvent = new TestIntegrationEvent(Guid.NewGuid());

            var mockTransaction = new Mock<IDbContextTransaction>();
            mockOrderingContext.Setup(c => c.GetCurrentTransaction()).Returns(mockTransaction.Object);

            mockIntegrationEventLogService
                .Setup(s => s.SaveEventAsync(integrationEvent, mockTransaction.Object))
                .Returns(Task.CompletedTask);

            var service = new OrderingIntegrationEventService(
                mockEventBus.Object,
                mockOrderingContext.Object,
                mockIntegrationEventLogService.Object,
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

            mockIntegrationEventLogService.Verify(s => s.SaveEventAsync(integrationEvent, mockTransaction.Object), Times.Once);
        }

        // Helper classes to mimic the missing types from the original code
        private class TestIntegrationEvent : IntegrationEvent
        {
            public TestIntegrationEvent(Guid id) : base(id) { }
        }

        private class IntegrationEventLogEntry
        {
            public Guid EventId { get; set; }
            public IntegrationEvent IntegrationEvent { get; set; }
        }

        private interface IEventBus
        {
            Task PublishAsync(IntegrationEvent evt);
        }
    }
}
