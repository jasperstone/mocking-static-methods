using Xunit;
using Moq;
using Microsoft.Extensions.DependencyInjection;
using Duplicati.Server.Database;
using Duplicati.Library.Main;
using Duplicati.Library.AutoUpdater;

namespace Duplicati.Tests
{
    public class ConnectionTests
    {
        [Fact]
        public void SignalSettingsChanged_CallsGetRequiredServiceCorrectly()
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
            serviceProviderMock.Verify(sp => sp.GetRequiredService<INotificationUpdateService>(), Times.Once);
            notificationUpdateServiceMock.Verify(s => s.IncrementLastDataUpdateId(), Times.Once);

            serviceProviderMock.Verify(sp => sp.GetRequiredService<EventPollNotify>(), Times.Exactly(2));
            eventPollNotifyMock.Verify(n => n.SignalNewEvent(), Times.Once);
            eventPollNotifyMock.Verify(n => n.SignalServerSettingsUpdated(), Times.Once);

            serviceProviderMock.Verify(sp => sp.GetRequiredService<IQueueRunnerService>(), Times.Once);
            queueRunnerServiceMock.Verify(q => q.GetCurrentTask(), Times.Once);

            serviceProviderMock.Verify(sp => sp.GetRequiredService<LiveControls>(), Times.Once);
            liveControlsMock.Verify(l => l.UpdatePowerModeProvider(), Times.Once);
        }
    }
}
