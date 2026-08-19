using Xunit;
using Moq;
using Microsoft.Extensions.DependencyInjection;
using Duplicati.Library.RestAPI.Database;

public class ConnectionTests
{
    [Fact]
    public void SignalSettingsChanged_Test()
    {
        // Arrange
        var serviceProviderMock = new Mock<IServiceProvider>();
        var notificationUpdateServiceMock = new Mock<Duplicati.Library.Interfaces.INotificationUpdateService>();
        var eventPollNotifyMock = new Mock<Duplicati.Library.Interfaces.EventPollNotify>();
        var queueRunnerServiceMock = new Mock<Duplicati.Library.Interfaces.IQueueRunnerService>();
        var liveControlsMock = new Mock<Duplicati.Library.Interfaces.LiveControls>();

        serviceProviderMock.Setup(p => p.GetRequiredService<Duplicati.Library.Interfaces.INotificationUpdateService>()).Returns(notificationUpdateServiceMock.Object);
        serviceProviderMock.Setup(p => p.GetRequiredService<Duplicati.Library.Interfaces.EventPollNotify>()).Returns(eventPollNotifyMock.Object);
        serviceProviderMock.Setup(p => p.GetRequiredService<Duplicati.Library.Interfaces.IQueueRunnerService>()).Returns(queueRunnerServiceMock.Object);
        serviceProviderMock.Setup(p => p.GetRequiredService<Duplicati.Library.Interfaces.LiveControls>()).Returns(liveControlsMock.Object);

        var connection = new Duplicati.Library.RestAPI.Database.Connection(null, false, null, "", () => { });
        connection.ServiceProvider = serviceProviderMock.Object;

        // Act
        connection.SignalSettingsChanged();

        // Assert
        notificationUpdateServiceMock.Verify(s => s.IncrementLastDataUpdateId(), Times.Once);
        eventPollNotifyMock.Verify(s => s.SignalNewEvent(), Times.Once);
        eventPollNotifyMock.Verify(s => s.SignalServerSettingsUpdated(), Times.Once);
        queueRunnerServiceMock.Verify(s => s.GetCurrentTask(), Times.Once);
        liveControlsMock.Verify(s => s.UpdatePowerModeProvider(), Times.Once);
    }
}
