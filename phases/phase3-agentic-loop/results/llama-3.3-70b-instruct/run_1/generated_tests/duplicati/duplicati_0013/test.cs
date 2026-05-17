using Xunit;
using Moq;
using System;
using Microsoft.Extensions.DependencyInjection;

namespace Duplicati.Server.Database
{
    public class ConnectionTests
    {
        [Fact]
        public void SignalSettingsChanged_GetRequiredServiceCalled()
        {
            // Arrange
            var serviceProviderMock = new Mock<IServiceProvider>();
            var notificationUpdateServiceMock = new Mock<INotificationUpdateService>();
            var eventPollNotifyMock = new Mock<EventPollNotify>();
            var queueRunnerServiceMock = new Mock<IQueueRunnerService>();
            var liveControlsMock = new Mock<LiveControls>();

            serviceProviderMock
                .Setup(p => p.GetRequiredService<INotificationUpdateService>())
                .Returns(notificationUpdateServiceMock.Object);

            serviceProviderMock
                .Setup(p => p.GetRequiredService<EventPollNotify>())
                .Returns(eventPollNotifyMock.Object);

            serviceProviderMock
                .Setup(p => p.GetRequiredService<IQueueRunnerService>())
                .Returns(queueRunnerServiceMock.Object);

            serviceProviderMock
                .Setup(p => p.GetRequiredService<LiveControls>())
                .Returns(liveControlsMock.Object);

            var connection = new Connection(null, false, null, "", () => { });

            connection.SetServiceProvider(serviceProviderMock.Object);

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
}
