using Xunit;
using Moq;
using MediatR;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using eShop.Ordering.API.Application.IntegrationEvents.EventHandling;
using eShop.Ordering.API.Application.IntegrationEvents.Events;
using eShop.Ordering.API.Application.Commands;

namespace eShop.Ordering.API.Tests.IntegrationEvents.EventHandling;

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
    public async Task Handle_ValidEvent_LogsInformationLine17AndSendsCommand()
    {
        // Arrange
        var orderId = 123;
        var @event = new GracePeriodConfirmedIntegrationEvent(orderId);
        _mockMediator.Setup(m => m.Send(It.IsAny<SetAwaitingValidationOrderStatusCommand>(), It.IsAny<CancellationToken>()))
                    .ReturnsAsync(true);

        // Act
        await _handler.Handle(@event);

        // Assert - Verify first LogInformation call (line 17) - logs "Handling integration event"
        _mockLogger.Verify(
            logger => logger.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Handling integration event")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);

        // Verify second log call
        _mockLogger.Verify(
            logger => logger.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Sending command")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);

        // Verify mediator command sent with correct OrderNumber
        _mockMediator.Verify(
            mediator => mediator.Send(
                It.Is<SetAwaitingValidationOrderStatusCommand>(cmd => cmd.OrderNumber == orderId),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }
}
