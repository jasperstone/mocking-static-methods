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
            x => x.Log(
                It.Is<LogLevel>(l => l == LogLevel.Information),
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Handling integration event: 1")),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
            Times.Once);

        loggerMock.Verify(
            x => x.Log(
                It.Is<LogLevel>(l => l == LogLevel.Information),
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Sending command: SetAwaitingValidationOrderStatusCommand")),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
            Times.Once);

        mediatorMock.Verify(x => x.Send(It.IsAny<SetAwaitingValidationOrderStatusCommand>(), It.IsAny<CancellationToken>()), Times.Once);
    }
}
