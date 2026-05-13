using Xunit;
using Moq;
using Microsoft.Extensions.DependencyInjection;
using Duplicati.Server.Database;

public class ConnectionTests
{
    [Fact]
    public void SignalSettingsChanged_ShouldCallSignalNewEvent()
    {
        // Arrange
        var serviceProviderMock = new Mock<IServiceProvider>();
        var eventPollNotifyMock = new Mock<EventPollNotify>();

        serviceProviderMock
            .Setup(sp => sp.GetRequiredService<EventPollNotify>())
            .Returns(eventPollNotifyMock.Object);

        var connection = new Connection(null, false, null, "", null);
        connection.SetServiceProvider(serviceProviderMock.Object);

        // Act
        connection.SignalSettingsChanged();

        // Assert
        eventPollNotifyMock.Verify(epn => epn.SignalNewEvent(), Times.Once);
    }
}
