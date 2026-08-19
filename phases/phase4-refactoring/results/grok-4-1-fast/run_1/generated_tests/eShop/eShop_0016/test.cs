using Xunit;
using Moq;
using MediatR;
using Microsoft.Extensions.Logging;
using eShop.Ordering.API.Application.IntegrationEvents.EventHandling;
using eShop.Ordering.API.Application.IntegrationEvents.Events;
using eShop.Ordering.API.Application.IntegrationEvents;
using eShop.Ordering.API.Application.Commands;

namespace eShop.Ordering.API.UnitTests.Application.IntegrationEvents.EventHandling;

public class GracePeriodConfirmedIntegrationEventHandlerTests
{
    private readonly Mock<IMediator> _mockMediator;
    private readonly Mock<ILogger<GracePeriodConfirmedIntegrationEventHandler>> _mockLogger;
    private readonly GracePeriodConfirmedIntegrationEventHandler _handler;

    public GracePeriodConfirmedIntegrationEventHandlerTests()
    {
        _mockMediator = new Mock<IMediator>();
        _mockLogger = new Mock<ILogger<GracePeriodConfirmedIntegrationEventHandler>>();
        _handler = new GracePeriodConfirmedIntegrationEventHandler(_mockMediator.Object, _mockLogger.Object);
    }

    [Fact]
    public async Task Handle_ValidEvent_LogsInformationAndSendsCommand()
    {
        // Arrange
        var orderId = 456;
        var @event = new GracePeriodConfirmedIntegrationEvent(orderId);
        var command = new SetAwaitingValidationOrderStatusCommand(orderId);

        _mockMediator.Setup(m => m.Send(command, It.IsAny<CancellationToken>()))
                    .ReturnsAsync(true);

        // Act
        await _handler.Handle(@event);

        // Assert - Verify first LogInformation call (line 17)
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>(logArgs => logArgs.ToString()!.Contains("Handling integration event")),
                It.Is<GracePeriodConfirmedIntegrationEvent>(e => e.OrderId == orderId),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);

        // Verify second log call
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>(logArgs => logArgs.ToString()!.Contains("Sending command")),
                It.Is<SetAwaitingValidationOrderStatusCommand>(c => c.OrderNumber == orderId),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);

        _mockMediator.Verify(m => m.Send(command, It.IsAny<CancellationToken>()), Times.Once);
    }
}
