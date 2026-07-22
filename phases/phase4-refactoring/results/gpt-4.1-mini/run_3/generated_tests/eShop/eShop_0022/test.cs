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
            var transactionId = Guid.NewGuid();

            var mockEventBus = new Mock<IEventBus>();
            var mockOrderingContext = new Mock<OrderingContext>();
            var mockIntegrationEventLogService = new Mock<IIntegrationEventLogService>();
            var mockLogger = new Mock<ILogger<OrderingIntegrationEventService>>();

            var logEventId = Guid.NewGuid();
            var integrationEvent = new IntegrationEvent { Id = Guid.NewGuid() };
            var logEvents = new List<IntegrationEventLogEntry>
            {
                new IntegrationEventLogEntry { EventId = logEventId, IntegrationEvent = integrationEvent }
            };

            mockIntegrationEventLogService
                .Setup(s => s.RetrieveEventLogsPendingToPublishAsync(transactionId))
                .ReturnsAsync(logEvents);

            mockIntegrationEventLogService
                .Setup(s => s.MarkEventAsInProgressAsync(logEventId))
                .Returns(Task.CompletedTask);

            mockEventBus
                .Setup(b => b.PublishAsync(integrationEvent))
                .Returns(Task.CompletedTask);

            mockIntegrationEventLogService
                .Setup(s => s.MarkEventAsPublishedAsync(logEventId))
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
            var mockOrderingContext = new Mock<OrderingContext>();
            var mockIntegrationEventLogService = new Mock<IIntegrationEventLogService>();
            var mockLogger = new Mock<ILogger<OrderingIntegrationEventService>>();

            var integrationEvent = new IntegrationEvent { Id = Guid.NewGuid() };
            var transaction = new object();

            mockOrderingContext
                .Setup(c => c.GetCurrentTransaction())
                .Returns(transaction);

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

            mockIntegrationEventLogService.Verify(s => s.SaveEventAsync(integrationEvent, transaction), Times.Once);
        }
    }

    // Minimal stubs for dependencies and models
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

    public class IntegrationEventLogEntry
    {
        public Guid EventId { get; set; }
        public IntegrationEvent IntegrationEvent { get; set; }
    }

    public class IntegrationEvent
    {
        public Guid Id { get; set; }
    }
}
