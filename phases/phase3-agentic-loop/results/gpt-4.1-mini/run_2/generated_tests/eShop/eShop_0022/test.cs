using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using eShop.Ordering.API.Application.IntegrationEvents;
using eShop.Ordering.Infrastructure;
using eShop.IntegrationEventLogEF.Services;
using eShop.EventBus.Abstractions;

namespace eShop.Ordering.API.Tests.Application.IntegrationEvents
{
    public class OrderingIntegrationEventServiceTests
    {
        [Fact]
        public async Task PublishEventsThroughEventBusAsync_LogsInformationForEachPendingEvent()
        {
            // Arrange
            var transactionId = Guid.NewGuid();

            var mockEventBus = new Mock<IEventBus>();
            var mockOrderingContext = new Mock<OrderingContext>(new object[] { null, null });
            var mockIntegrationEventLogService = new Mock<IIntegrationEventLogService>();
            var mockLogger = new Mock<ILogger<OrderingIntegrationEventService>>();

            var integrationEvent = new IntegrationEvent(Guid.NewGuid());
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
        public async Task AddAndSaveEventAsync_LogsInformationAndSavesEvent()
        {
            // Arrange
            var mockEventBus = new Mock<IEventBus>();
            var mockOrderingContext = new Mock<OrderingContext>(new object[] { null, null });
            var mockIntegrationEventLogService = new Mock<IIntegrationEventLogService>();
            var mockLogger = new Mock<ILogger<OrderingIntegrationEventService>>();

            var integrationEvent = new IntegrationEvent(Guid.NewGuid());

            mockIntegrationEventLogService
                .Setup(s => s.SaveEventAsync(integrationEvent, It.IsAny<object>()))
                .Returns(Task.CompletedTask);

            mockOrderingContext
                .Setup(c => c.GetCurrentTransaction())
                .Returns(new object());

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

            mockIntegrationEventLogService.Verify(
                s => s.SaveEventAsync(integrationEvent, It.IsAny<object>()),
                Times.Once);
        }
    }
}
