using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using eShop.Ordering.API.Application.IntegrationEvents;
using eShop.Services.Ordering.Ordering.Infrastructure;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace eShop.Tests.Ordering.Application.IntegrationEvents
{
    public class OrderingIntegrationEventServiceTests
    {
        [Fact]
        public async Task PublishEventsThroughEventBusAsync_LogsInformationForEachPendingEvent()
        {
            // Arrange
            var transactionId = Guid.NewGuid();

            var integrationEvent = new TestIntegrationEvent();
            var logEntry = new IntegrationEventLogEntry(integrationEvent, Guid.NewGuid(), nameof(TestIntegrationEvent), DateTime.UtcNow);
            var pendingEvents = new List<IntegrationEventLogEntry> { logEntry };

            var eventBusMock = new Mock<IEventBus>();
            eventBusMock.Setup(b => b.PublishAsync(integrationEvent)).Returns(Task.CompletedTask);

            var orderingContextMock = new Mock<OrderingContext>(MockBehavior.Strict, new object?[] { null! });

            var eventLogServiceMock = new Mock<IIntegrationEventLogService>();
            eventLogServiceMock
                .Setup(s => s.RetrieveEventLogsPendingToPublishAsync(transactionId))
                .ReturnsAsync(pendingEvents);
            eventLogServiceMock.Setup(s => s.MarkEventAsInProgressAsync(logEntry.EventId)).Returns(Task.CompletedTask);
            eventLogServiceMock.Setup(s => s.MarkEventAsPublishedAsync(logEntry.EventId)).Returns(Task.CompletedTask);

            var loggerMock = new Mock<ILogger<OrderingIntegrationEventService>>();

            var service = new OrderingIntegrationEventService(
                eventBusMock.Object,
                orderingContextMock.Object,
                eventLogServiceMock.Object,
                loggerMock.Object);

            // Act
            await service.PublishEventsThroughEventBusAsync(transactionId);

            // Assert
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((o, _) => o.ToString()!.Contains("Publishing integration event")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [Fact]
        public async Task AddAndSaveEventAsync_LogsInformationWhenEventQueued()
        {
            // Arrange
            var integrationEvent = new TestIntegrationEvent();
            var transaction = new object();

            var eventBusMock = new Mock<IEventBus>();

            var orderingContextMock = new Mock<OrderingContext>(MockBehavior.Strict, new object?[] { null! });
            orderingContextMock.Setup(c => c.GetCurrentTransaction()).Returns(transaction);

            var eventLogServiceMock = new Mock<IIntegrationEventLogService>();
            eventLogServiceMock.Setup(s => s.SaveEventAsync(integrationEvent, transaction))
                .Returns(Task.CompletedTask);

            var loggerMock = new Mock<ILogger<OrderingIntegrationEventService>>();

            var service = new OrderingIntegrationEventService(
                eventBusMock.Object,
                orderingContextMock.Object,
                eventLogServiceMock.Object,
                loggerMock.Object);

            // Act
            await service.AddAndSaveEventAsync(integrationEvent);

            // Assert
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((o, _) => o.ToString()!.Contains("Enqueuing integration event")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);

            eventLogServiceMock.Verify(
                s => s.SaveEventAsync(integrationEvent, transaction),
                Times.Once);
        }

        private sealed class TestIntegrationEvent : IntegrationEvent
        {
        }
    }
}
