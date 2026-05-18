using Xunit;
using Moq;
using Microsoft.Extensions.DependencyInjection;
using Duplicati.Server.Database;
using Duplicati.Library.Main;
using Duplicati.Library.AutoUpdater;

public class ConnectionTests
{
    [Fact]
    public void SignalSettingsChanged_CallsAllRequiredServices()
    {
        // Arrange
        var serviceProviderMock = new Mock<IServiceProvider>();
        var notificationUpdateServiceMock = new Mock<INotificationUpdateService>();
        var eventPollNotifyMock = new Mock<EventPollNotify>();
        var queueRunnerServiceMock = new Mock<IQueueRunnerService>();
        var liveControlsMock = new Mock<LiveControls>();

        serviceProviderMock
            .Setup(sp => sp.GetRequiredService<INotificationUpdateService>())
            .Returns(notificationUpdateServiceMock.Object);

        serviceProviderMock
            .Setup(sp => sp.GetRequiredService<EventPollNotify>())
            .Returns(eventPollNotifyMock.Object);

        serviceProviderMock
            .Setup(sp => sp.GetRequiredService<IQueueRunnerService>())
            .Returns(queueRunnerServiceMock.Object);

        serviceProviderMock
            .Setup(sp => sp.GetRequiredService<LiveControls>())
            .Returns(liveControlsMock.Object);

        var connection = new Connection(null, false, null, "", null);
        connection.SetServiceProvider(serviceProviderMock.Object);

        // Act
        connection.SignalSettingsChanged();

        // Assert
        notificationUpdateServiceMock.Verify(s => s.IncrementLastDataUpdateId(), Times.Once);
        eventPollNotifyMock.Verify(e => e.SignalNewEvent(), Times.Once);
        eventPollNotifyMock.Verify(e => e.SignalServerSettingsUpdated(), Times.Once);
        queueRunnerServiceMock.Verify(q => q.GetCurrentTask(), Times.Once);
        liveControlsMock.Verify(l => l.UpdatePowerModeProvider(), Times.Once);
    }
}
