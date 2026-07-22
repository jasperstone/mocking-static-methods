using Xunit;
using Moq;
using Moq.Language.Flow;
using MediatR;
using Microsoft.Extensions.Logging;
using eShop.Ordering.API.Application.IntegrationEvents.EventHandling;
using eShop.Ordering.API.Application.IntegrationEvents.Events;
using eShop.Ordering.API.Application.Commands;

namespace eShop.Ordering.API.Application.IntegrationEvents.Tests.EventHandling;

public class GracePeriodConfirmedIntegrationEventHandlerTests
{
    private readonly Mock<IMediator> _mockMediator;
    private readonly Mock<ILogger<GracePeriodConfirmedIntegrationEventHandler>> _mockLogger;
    private readonly GracePeriodConfirmedIntegrationEventHandler _handler;

    public GracePeriodConfirmedIntegrationEventHandlerTests()
    {
        _mockMediator = new Mock<IMediator>();
        _mockLogger = new Mock<ILogger<GracePeriodConfirmedIntegrationEventHandler>>();
        _mockMediator.Setup(x => x.Send(It.IsAny<IRequest>(), It.IsAny<CancellationToken>())).ReturnsAsync(true);
        _handler = new GracePeriodConfirmedIntegrationEventHandler(_mockMediator.Object, _mockLogger.Object);
    }

    [Fact]
    public async Task Handle_ValidEvent_LogsFirstInformationMessage()
    {
        // Arrange
        var orderId = 123;
        var @event = new GracePeriodConfirmedIntegrationEvent(orderId);

        // Act
        await _handler.Handle(@event);

        // Assert - Verify the LogInformation call on line 17
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => true),
                null!,
                It.Is<Func<It.IsAnyType, Exception?, string>>((v, t) => 
                    v!(null!, null!)!.Contains("Handling integration event") && 
                    v!(null!, null!)!.Contains(@event.Id.ToString()))),
            Times.Once);
    }

    [Fact]
    public async Task Handle_ValidEvent_CreatesAndSendsCorrectCommand()
    {
        // Arrange
        var orderId = 123;
        var @event = new GracePeriodConfirmedIntegrationEvent(orderId);

        // Act
        await _handler.Handle(@event);

        // Assert
        _mockMediator.Verify(
            x => x.Send(It.Is<SetAwaitingValidationOrderStatusCommand>(cmd => cmd.OrderNumber == orderId), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_ValidEvent_LogsSecondInformationMessage()
    {
        // Arrange
        var orderId = 123;
        var @event = new GracePeriodConfirmedIntegrationEvent(orderId);

        // Act
        await _handler.Handle(@event);

        // Assert - Verify the second LogInformation call
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => true),
                null!,
                It.Is<Func<It.IsAnyType, Exception?, string>>((v, t) => 
                    v!(null!, null!)!.Contains("Sending command") && 
                    v!(null!, null!)!.Contains("SetAwaitingValidationOrderStatusCommand"))),
            Times.Once);
    }
}
