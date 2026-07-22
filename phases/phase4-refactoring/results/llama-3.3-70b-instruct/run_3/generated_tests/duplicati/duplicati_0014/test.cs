using Xunit;
using Moq;
using Duplicati.Library;
using Microsoft.Extensions.DependencyInjection;
using System;
using Duplicati.Server.Database;

public class ConnectionTests
{
    [Fact]
    public void SignalSettingsChanged_GetRequiredService_EventPollNotify_SignalNewEvent()
    {
        // Arrange
        var providerMock = new Mock<IServiceProvider>();
        var eventPollNotifyMock = new Mock<Duplicati.Library.Backend.EventPollNotify>();
        providerMock.Setup(p => p.GetRequiredService<Duplicati.Library.Backend.EventPollNotify>()).Returns(eventPollNotifyMock.Object);

        var connection = new Connection(null, false, null, "", () => { });
        connection.SetServiceProvider(providerMock.Object);

        // Act
        connection.SignalSettingsChanged();

        // Assert
        eventPollNotifyMock.Verify(e => e.SignalNewEvent(), Times.Once);
    }

    [Fact]
    public void SignalSettingsChanged_GetRequiredService_EventPollNotify_SignalServerSettingsUpdated()
    {
        // Arrange
        var providerMock = new Mock<IServiceProvider>();
        var eventPollNotifyMock = new Mock<Duplicati.Library.Backend.EventPollNotify>();
        providerMock.Setup(p => p.GetRequiredService<Duplicati.Library.Backend.EventPollNotify>()).Returns(eventPollNotifyMock.Object);

        var connection = new Connection(null, false, null, "", () => { });
        connection.SetServiceProvider(providerMock.Object);

        // Act
        connection.SignalSettingsChanged();

        // Assert
        eventPollNotifyMock.Verify(e => e.SignalServerSettingsUpdated(), Times.Once);
    }

    [Fact]
    public void SignalSettingsChanged_GetRequiredService_INotificationUpdateService_IncrementLastDataUpdateId()
    {
        // Arrange
        var providerMock = new Mock<IServiceProvider>();
        var notificationUpdateServiceMock = new Mock<Duplicati.Library.Interfaces.INotificationUpdateService>();
        providerMock.Setup(p => p.GetRequiredService<Duplicati.Library.Interfaces.INotificationUpdateService>()).Returns(notificationUpdateServiceMock.Object);

        var connection = new Connection(null, false, null, "", () => { });
        connection.SetServiceProvider(providerMock.Object);

        // Act
        connection.SignalSettingsChanged();

        // Assert
        notificationUpdateServiceMock.Verify(n => n.IncrementLastDataUpdateId(), Times.Once);
    }
}
