using Xunit;
using Moq;
using MediatR;
using Microsoft.Extensions.Logging;
using eShop.Ordering.API.Application.IntegrationEvents.EventHandling;
using eShop.Ordering.API.Application.IntegrationEvents.Events;

public class GracePeriodConfirmedIntegrationEventHandlerTests
{
    [Fact]
    public async Task Handle_ShouldLogInformation()
    {
        // Arrange
        var mediatorMock = new Mock<IMediator>();
        var loggerMock = new Mock<ILogger<GracePeriodConfirmedIntegrationEventHandler>>();

        var handler = new GracePeriodConfirmedIntegrationEventHandler(mediatorMock.Object, loggerMock.Object);
        var eventData = new GracePeriodConfirmedIntegrationEvent(1);

        // Act
        await handler.Handle(eventData);

        // Assert
        loggerMock.Verify(
            x => x.Log(
                It.Is<LogLevel>(l => l == LogLevel.Information),
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Handling integration event")),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
            Times.Once);
    }
}
