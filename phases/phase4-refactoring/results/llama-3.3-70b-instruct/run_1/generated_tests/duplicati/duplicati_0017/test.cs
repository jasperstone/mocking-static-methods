using Xunit;
using Moq;
using System;
using Duplicati.Library;

namespace Duplicati.Library.Tests
{
    public class ConnectionTests
    {
        [Fact]
        public void SignalSettingsChanged_Test()
        {
            // Arrange
            var providerMock = new Mock<IServiceProvider>();
            var notificationUpdateServiceMock = new Mock<INotificationUpdateService>();
            var eventPollNotifyMock = new Mock<EventPollNotify>();
            var queueRunnerServiceMock = new Mock<IQueueRunnerService>();
            var liveControlsMock = new Mock<LiveControls>();

            providerMock.Setup(p => p.GetService(typeof(INotificationUpdateService))).Returns(notificationUpdateServiceMock.Object);
            providerMock.Setup(p => p.GetService(typeof(EventPollNotify))).Returns(eventPollNotifyMock.Object);
            providerMock.Setup(p => p.GetService(typeof(IQueueRunnerService))).Returns(queueRunnerServiceMock.Object);
            providerMock.Setup(p => p.GetService(typeof(LiveControls))).Returns(liveControlsMock.Object);

            var connection = new Duplicati.Library.RestAPI.Database.Connection();
            connection.ServiceProvider = providerMock.Object;

            // Act
            connection.SignalSettingsChanged();

            // Assert
            notificationUpdateServiceMock.Verify(n => n.IncrementLastDataUpdateId(), Times.Once);
            eventPollNotifyMock.Verify(e => e.SignalNewEvent(), Times.Once);
            eventPollNotifyMock.Verify(e => e.SignalServerSettingsUpdated(), Times.Once);
            queueRunnerServiceMock.Verify(q => q.GetCurrentTask(), Times.Once);
            liveControlsMock.Verify(l => l.UpdatePowerModeProvider(), Times.Once);
        }
    }
}
