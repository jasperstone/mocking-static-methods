using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using MediatR;
using eShop.Ordering.API.Application.IntegrationEvents.EventHandling;
using eShop.Ordering.API.Application.IntegrationEvents.Events;
using eShop.Ordering.API.Application.Commands;

namespace eShop.Ordering.API.UnitTests.Application.IntegrationEvents.EventHandling;

public class GracePeriodConfirmedIntegrationEventHandlerTests
{
    private readonly Mock<ILogger<GracePeriodConfirmedIntegrationEventHandler>> _mockLogger;
    private readonly Mock<IMediator> _mockMediator;
    private readonly GracePeriodConfirmedIntegrationEventHandler _handler;

    public GracePeriodConfirmedIntegrationEventHandlerTests()
    {
        _mockLogger = new Mock<ILogger<GracePeriodConfirmedIntegrationEventHandler>>();
        _mockMediator = new Mock<IMediator>();
        _handler = new GracePeriodConfirmedIntegrationEventHandler(_mockMediator.Object, _mockLogger.Object);
    }

    [Fact]
    public async Task Handle_ValidEvent_LogsInformationAndSendsCommand()
    {
        // Arrange
        var orderId = 123;
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
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Handling integration event") && v.ToString().Contains(@event.Id.ToString())),
                @event,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);

        // Assert - Verify second LogInformation call
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Sending command") && v.ToString().Contains("SetAwaitingValidationOrderStatusCommand")),
                command,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);

        _mockMediator.Verify(m => m.Send(command, It.IsAny<CancellationToken>()), Times.Once);
    }
}
