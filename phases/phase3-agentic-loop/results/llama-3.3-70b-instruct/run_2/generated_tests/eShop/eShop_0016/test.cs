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
    public async Task Handle_LogsInformation_WhenHandlingIntegrationEvent()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<GracePeriodConfirmedIntegrationEventHandler>>();
        var mediatorMock = new Mock<IMediator>();
        var handler = new GracePeriodConfirmedIntegrationEventHandler(mediatorMock.Object, loggerMock.Object);
        var @event = new GracePeriodConfirmedIntegrationEvent(1);

        // Act
        await handler.Handle(@event);

        // Assert
        loggerMock.Verify(l => l.LogInformation(It.IsAny<string>(), @event.Id, @event), Times.Once);
    }

    [Fact]
    public async Task Handle_SendsCommand_WhenHandlingIntegrationEvent()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<GracePeriodConfirmedIntegrationEventHandler>>();
        var mediatorMock = new Mock<IMediator>();
        var handler = new GracePeriodConfirmedIntegrationEventHandler(mediatorMock.Object, loggerMock.Object);
        var @event = new GracePeriodConfirmedIntegrationEvent(1);

        // Act
        await handler.Handle(@event);

        // Assert
        mediatorMock.Verify(m => m.Send(It.IsAny<object>(), default), Times.Once);
    }
}
