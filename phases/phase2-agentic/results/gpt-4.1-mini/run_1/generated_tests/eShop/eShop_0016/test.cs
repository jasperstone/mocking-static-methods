using System.Threading.Tasks;
using eShop.Ordering.API.Application.IntegrationEvents.EventHandling;
using eShop.Ordering.API.Application.IntegrationEvents.Events;
using MediatR;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace eShop.Ordering.API.Tests.Application.IntegrationEvents.EventHandling;

public class GracePeriodConfirmedIntegrationEventHandlerTests
{
    [Fact]
    public async Task Handle_LogsInformationAndSendsCommand()
    {
        // Arrange
        var mediatorMock = new Mock<IMediator>();
        var loggerMock = new Mock<ILogger<GracePeriodConfirmedIntegrationEventHandler>>();

        var handler = new GracePeriodConfirmedIntegrationEventHandler(mediatorMock.Object, loggerMock.Object);

        var @event = new GracePeriodConfirmedIntegrationEvent(orderId: 123);

        // Act
        await handler.Handle(@event);

        // Assert
        // Verify the first LogInformation call with the event info
        loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Handling integration event")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);

        // Verify the second LogInformation call with the command info
        loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Sending command")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);

        // Verify mediator.Send was called once with a SetAwaitingValidationOrderStatusCommand with correct OrderId
        mediatorMock.Verify(m => m.Send(
            It.Is<SetAwaitingValidationOrderStatusCommand>(cmd => cmd.OrderNumber == 123),
            default), Times.Once);
    }
}
