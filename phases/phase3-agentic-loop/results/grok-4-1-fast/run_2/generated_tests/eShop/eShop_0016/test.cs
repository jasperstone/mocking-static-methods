using Xunit;
using Moq;
using MediatR;
using Microsoft.Extensions.Logging;
using eShop.Ordering.API.Application.IntegrationEvents.EventHandling;
using eShop.Ordering.API.Application.IntegrationEvents.Events;
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
        var orderId = 123;
        var @event = new GracePeriodConfirmedIntegrationEvent(orderId);
        _mockMediator.Setup(m => m.Send(It.IsAny<SetAwaitingValidationOrderStatusCommand>(), It.IsAny<CancellationToken>()))
                    .ReturnsAsync(true);

        // Act
        await _handler.Handle(@event);

        // Assert - Verify the specific LogInformation call on line 17
        _mockLogger.Verify(
            logger => logger.LogInformation(
                "Handling integration event: {IntegrationEventId} - ({@IntegrationEvent})",
                @event.Id,
                @event),
            Times.Once);

        // Verify command is sent with correct OrderNumber
        _mockMediator.Verify(
            mediator => mediator.Send(
                It.Is<SetAwaitingValidationOrderStatusCommand>(cmd => cmd.OrderNumber == orderId),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }
}
