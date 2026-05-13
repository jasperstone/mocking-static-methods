using Xunit;
using Moq;
using System;
using Duplicati.Library.Main;
using Microsoft.Extensions.DependencyInjection;

namespace Duplicati.Tests
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

            var connection = new Connection(null, false, null, "", () => { });
            connection.SetServiceProvider(serviceProviderMock.Object);

            // Act
            connection.SignalSettingsChanged();

            // Assert
            notificationUpdateServiceMock.Verify(nus => nus.IncrementLastDataUpdateId(), Times.Once);
            eventPollNotifyMock.Verify(epn => epn.SignalNewEvent(), Times.Once);
            eventPollNotifyMock.Verify(epn => epn.SignalServerSettingsUpdated(), Times.Once);
            queueRunnerServiceMock.Verify(qrs => qrs.GetCurrentTask(), Times.Once);
            liveControlsMock.Verify(lc => lc.UpdatePowerModeProvider(), Times.Once);
        }
    }
}
