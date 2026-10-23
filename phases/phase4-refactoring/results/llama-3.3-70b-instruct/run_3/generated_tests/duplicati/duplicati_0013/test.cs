using Xunit;
using Moq;
using Duplicati.Library.RestAPI.Database;
using Microsoft.Extensions.DependencyInjection;

namespace Duplicati.Tests
{
    public class ConnectionTests
    {
        [Fact]
        public void SignalSettingsChanged_CallsIncrementLastDataUpdateId()
        {
            // Arrange
            var serviceProviderMock = new Mock<IServiceProvider>();
            var notificationUpdateServiceMock = new Mock<INotificationUpdateService>();
            serviceProviderMock.Setup(p => p.GetRequiredService<INotificationUpdateService>()).Returns(notificationUpdateServiceMock.Object);

            var connection = new Connection(null, false, null, "", () => { });
            connection.SetServiceProvider(serviceProviderMock.Object);

            // Act
            connection.SignalSettingsChanged();

            // Assert
            notificationUpdateServiceMock.Verify(s => s.IncrementLastDataUpdateId(), Times.Once);
        }

        [Fact]
        public void SignalSettingsChanged_CallsSignalNewEvent()
        {
            // Arrange
            var serviceProviderMock = new Mock<IServiceProvider>();
            var eventPollNotifyMock = new Mock<EventPollNotify>();
            serviceProviderMock.Setup(p => p.GetRequiredService<EventPollNotify>()).Returns(eventPollNotifyMock.Object);

            var connection = new Connection(null, false, null, "", () => { });
            connection.SetServiceProvider(serviceProviderMock.Object);

            // Act
            connection.SignalSettingsChanged();

            // Assert
            eventPollNotifyMock.Verify(s => s.SignalNewEvent(), Times.Once);
        }

        [Fact]
        public void SignalSettingsChanged_CallsSignalServerSettingsUpdated()
        {
            // Arrange
            var serviceProviderMock = new Mock<IServiceProvider>();
            var eventPollNotifyMock = new Mock<EventPollNotify>();
            serviceProviderMock.Setup(p => p.GetRequiredService<EventPollNotify>()).Returns(eventPollNotifyMock.Object);

            var connection = new Connection(null, false, null, "", () => { });
            connection.SetServiceProvider(serviceProviderMock.Object);

            // Act
            connection.SignalSettingsChanged();

            // Assert
            eventPollNotifyMock.Verify(s => s.SignalServerSettingsUpdated(), Times.Once);
        }
    }
}
