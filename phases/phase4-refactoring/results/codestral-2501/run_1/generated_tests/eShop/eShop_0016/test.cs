using Xunit;
using Moq;
using MediatR;
using Microsoft.Extensions.Logging;
using eShop.Ordering.API.Application.IntegrationEvents.EventHandling;
using eShop.Ordering.API.Application.IntegrationEvents.Events;
using eShop.Ordering.API.Application.Commands;

public class GracePeriodConfirmedIntegrationEventHandlerTests
{
    [Fact]
    public async Task Handle_LogsInformationAndSendsCommand()
    {
        // Arrange
        var mediatorMock = new Mock<IMediator>();
        var loggerMock = new Mock<ILogger<GracePeriodConfirmedIntegrationEventHandler>>();

        var handler = new GracePeriodConfirmedIntegrationEventHandler(mediatorMock.Object, loggerMock.Object);
        var integrationEvent = new GracePeriodConfirmedIntegrationEvent(1);

        // Act
        await handler.Handle(integrationEvent);

        // Assert
        loggerMock.Verify(
            x => x.LogInformation(
                "Handling integration event: {IntegrationEventId} - ({@IntegrationEvent})",
                integrationEvent.Id,
                integrationEvent),
            Times.Once);

        loggerMock.Verify(
            x => x.LogInformation(
                "Sending command: {CommandName} - {IdProperty}: {CommandId} ({@Command})",
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<int>(),
                It.IsAny<SetAwaitingValidationOrderStatusCommand>()),
            Times.Once);

        mediatorMock.Verify(
            x => x.Send(It.IsAny<SetAwaitingValidationOrderStatusCommand>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }
}
