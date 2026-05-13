using Xunit;
using Moq;
using Duplicati.Library.RestAPI.Database;
using Duplicati.Library.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace Duplicati.Tests
{
    public class ConnectionTests
    {
        [Fact]
        public void SignalSettingsChanged_GetRequiredService_INotificationUpdateService_IncrementLastDataUpdateId()
        {
            // Arrange
            var serviceProviderMock = new Mock<IServiceProvider>();
            var notificationUpdateServiceMock = new Mock<INotificationUpdateService>();
            serviceProviderMock.Setup(p => p.GetRequiredService<INotificationUpdateService>()).Returns(notificationUpdateServiceMock.Object);

            var connection = new Connection(serviceProviderMock.Object);

            // Act
            connection.SignalSettingsChanged();

            // Assert
            notificationUpdateServiceMock.Verify(s => s.IncrementLastDataUpdateId(), Times.Once);
        }

        [Fact]
        public void SignalSettingsChanged_GetRequiredService_EventPollNotify_SignalNewEvent()
        {
            // Arrange
            var serviceProviderMock = new Mock<IServiceProvider>();
            var eventPollNotifyMock = new Mock<EventPollNotify>();
            serviceProviderMock.Setup(p => p.GetRequiredService<EventPollNotify>()).Returns(eventPollNotifyMock.Object);

            var connection = new Connection(serviceProviderMock.Object);

            // Act
            connection.SignalSettingsChanged();

            // Assert
            eventPollNotifyMock.Verify(e => e.SignalNewEvent(), Times.Once);
        }

        [Fact]
        public void SignalSettingsChanged_GetRequiredService_EventPollNotify_SignalServerSettingsUpdated()
        {
            // Arrange
            var serviceProviderMock = new Mock<IServiceProvider>();
            var eventPollNotifyMock = new Mock<EventPollNotify>();
            serviceProviderMock.Setup(p => p.GetRequiredService<EventPollNotify>()).Returns(eventPollNotifyMock.Object);

            var connection = new Connection(serviceProviderMock.Object);

            // Act
            connection.SignalSettingsChanged();

            // Assert
            eventPollNotifyMock.Verify(e => e.SignalServerSettingsUpdated(), Times.Once);
        }
    }
}
