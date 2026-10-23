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
        [Fact]
        public async Task PublishEventsThroughEventBusAsync_LogsInformationForEachEvent()
        {
            // Arrange
            var mockEventBus = new Mock<IEventBus>();
            var mockOrderingContext = new Mock<OrderingContext>();
            var mockIntegrationEventLogService = new Mock<IIntegrationEventLogService>();
            var mockLogger = new Mock<ILogger<OrderingIntegrationEventService>>();

            var transactionId = Guid.NewGuid();

            var integrationEvent = new TestIntegrationEvent(Guid.NewGuid());
            var logEvent = new IntegrationEventLogEntry
            {
                EventId = Guid.NewGuid(),
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
        public async Task AddAndSaveEventAsync_LogsInformation()
        {
            // Arrange
            var mockEventBus = new Mock<IEventBus>();
            var mockOrderingContext = new Mock<OrderingContext>();
            var mockIntegrationEventLogService = new Mock<IIntegrationEventLogService>();
            var mockLogger = new Mock<ILogger<OrderingIntegrationEventService>>();

            var transaction = new Mock<IDisposable>().Object;
            mockOrderingContext.Setup(c => c.GetCurrentTransaction()).Returns(transaction);

            var integrationEvent = new TestIntegrationEvent(Guid.NewGuid());

            mockIntegrationEventLogService
                .Setup(s => s.SaveEventAsync(integrationEvent, transaction))
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
        }

        // Minimal stub for IntegrationEvent to allow compilation
        public class IntegrationEvent
        {
            public Guid Id { get; }

            public IntegrationEvent(Guid id)
            {
                Id = id;
            }
        }

        // Helper classes to simulate dependencies and data structures
        private class TestIntegrationEvent : IntegrationEvent
        {
            public TestIntegrationEvent(Guid id) : base(id) { }
        }

        private class IntegrationEventLogEntry
        {
            public Guid EventId { get; set; }
            public IntegrationEvent IntegrationEvent { get; set; }
        }
    }
}
