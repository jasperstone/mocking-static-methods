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

            var integrationEvent = new IntegrationEvent(Guid.NewGuid());
            var logEvent = new IntegrationEventLogEntry
            {
                EventId = Guid.NewGuid(),
                IntegrationEvent = integrationEvent
            };

            var pendingEvents = new List<IntegrationEventLogEntry> { logEvent };

            mockIntegrationEventLogService
                .Setup(s => s.RetrieveEventLogsPendingToPublishAsync(transactionId))
                .ReturnsAsync(pendingEvents);

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
    }

    // Minimal stubs for dependencies and models to allow compilation of the test
    public interface IEventBus
    {
        Task PublishAsync(IntegrationEvent evt);
    }

    public interface IIntegrationEventLogService
    {
        Task<IReadOnlyCollection<IntegrationEventLogEntry>> RetrieveEventLogsPendingToPublishAsync(Guid transactionId);
        Task MarkEventAsInProgressAsync(Guid eventId);
        Task MarkEventAsPublishedAsync(Guid eventId);
        Task MarkEventAsFailedAsync(Guid eventId);
        Task SaveEventAsync(IntegrationEvent evt, object transaction);
    }

    public class OrderingContext
    {
        public object GetCurrentTransaction() => null;
    }

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
}
