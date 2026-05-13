using Xunit;
using Moq;
using Duplicati.Library.RestAPI.Database;
using System;

namespace Duplicati.Tests
{
    public class ConnectionTests
    {
        [Fact]
        public void SignalSettingsChanged_GetRequiredServiceCalled()
        {
            // Arrange
            var providerMock = new Mock<IServiceProvider>();
            var notificationUpdateServiceMock = new Mock<INotificationUpdateService>();
            var eventPollNotifyMock = new Mock<EventPollNotify>();
            var queueRunnerServiceMock = new Mock<IQueueRunnerService>();
            var liveControlsMock = new Mock<LiveControls>();

            providerMock.Setup(p => p.GetRequiredService<INotificationUpdateService>()).Returns(notificationUpdateServiceMock.Object);
            providerMock.Setup(p => p.GetRequiredService<EventPollNotify>()).Returns(eventPollNotifyMock.Object);
            providerMock.Setup(p => p.GetRequiredService<IQueueRunnerService>()).Returns(queueRunnerServiceMock.Object);
            providerMock.Setup(p => p.GetRequiredService<LiveControls>()).Returns(liveControlsMock.Object);

            var connection = new Connection();
            connection.ServiceProvider = providerMock.Object;

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
}
