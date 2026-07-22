using Xunit;
using Moq;
using Moq.Language.Flow;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using eShop.Ordering.API.Application.IntegrationEvents;

namespace eShop.Ordering.API.UnitTests.Application.IntegrationEvents;

public class OrderingIntegrationEventServiceTests
{
    private readonly Mock<ILogger<OrderingIntegrationEventService>> _mockLogger;
    private readonly OrderingIntegrationEventService _service;

    public OrderingIntegrationEventServiceTests()
    {
        _mockLogger = new Mock<ILogger<OrderingIntegrationEventService>>();
        // Create minimal mocks with necessary members only
        var mockEventBus = new Mock<IEventBus>();
        var mockOrderingContext = new Mock<OrderingContext>();
        var mockEventLogService = new Mock<IIntegrationEventLogService>();

        _service = new OrderingIntegrationEventService(
            mockEventBus.Object,
            mockOrderingContext.Object,
            mockEventLogService.Object,
            _mockLogger.Object);
    }

    [Fact]
    public async Task PublishEventsThroughEventBusAsync_WhenCalled_LogsInformationMessage()
    {
        // Arrange - Setup minimal mocks to avoid exceptions
        var transactionId = Guid.NewGuid();
        var mockEventLogService = Mock.Get(_service.GetType().GetField("_eventLogService", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.GetValue(_service) as IIntegrationEventLogService);
        
        if (mockEventLogService != null)
        {
            mockEventLogService.Setup(x => x.RetrieveEventLogsPendingToPublishAsync(transactionId))
                .ReturnsAsync(new List<object>());
        }

        // Act
        await _service.PublishEventsThroughEventBusAsync(transactionId);

        // Assert - Verify LogInformation was called (line 19 coverage)
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => true),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.AtLeastOnce);
    }

    [Fact]
    public async Task AddAndSaveEventAsync_WhenCalled_LogsInformationMessage()
    {
        // Arrange
        dynamic fakeEvent = new { Id = Guid.NewGuid() };

        // Setup mock to avoid exceptions during call
        var mockEventLogService = Mock.Get(_service.GetType().GetField("_eventLogService", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.GetValue(_service) as IIntegrationEventLogService);
        var mockOrderingContext = Mock.Get(_service.GetType().GetField("_orderingContext", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.GetValue(_service) as OrderingContext);
        
        if (mockEventLogService != null)
            mockEventLogService.Setup(x => x.SaveEventAsync(It.IsAny<object>(), It.IsAny<object>())).Returns(Task.CompletedTask);

        // Act
        await _service.AddAndSaveEventAsync(fakeEvent);

        // Assert - Verify the other LogInformation call was hit
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => true),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.AtLeastOnce);
    }
}
