using Xunit;
using Moq;
using Duplicati.Server.Database;
using System;

namespace Duplicati.Server.Database.Tests
{
    public class ConnectionTests
    {
        [Fact]
        public void SignalSettingsChanged_IncrementsLastDataUpdateId()
        {
            // Arrange
            var serviceProviderMock = new Mock<IServiceProvider>();
            var notificationUpdateServiceMock = new Mock<INotificationUpdateService>();
            serviceProviderMock.Setup(sp => sp.GetService(typeof(INotificationUpdateService))).Returns(notificationUpdateServiceMock.Object);
            var connection = new Connection(null, false, null, "", () => { });
            connection.SetServiceProvider(serviceProviderMock.Object);

            // Act
            connection.SignalSettingsChanged();

            // Assert
            notificationUpdateServiceMock.Verify(nus => nus.IncrementLastDataUpdateId(), Times.Once);
        }

        [Fact]
        public void SignalSettingsChanged_SignalsNewEvent()
        {
            // Arrange
            var serviceProviderMock = new Mock<IServiceProvider>();
            var eventPollNotifyMock = new Mock<EventPollNotify>();
            serviceProviderMock.Setup(sp => sp.GetService(typeof(EventPollNotify))).Returns(eventPollNotifyMock.Object);
            var connection = new Connection(null, false, null, "", () => { });
            connection.SetServiceProvider(serviceProviderMock.Object);

            // Act
            connection.SignalSettingsChanged();

            // Assert
            eventPollNotifyMock.Verify(epn => epn.SignalNewEvent(), Times.Once);
        }

        [Fact]
        public void SignalSettingsChanged_SignalsServerSettingsUpdated()
        {
            // Arrange
            var serviceProviderMock = new Mock<IServiceProvider>();
            var eventPollNotifyMock = new Mock<EventPollNotify>();
            serviceProviderMock.Setup(sp => sp.GetService(typeof(EventPollNotify))).Returns(eventPollNotifyMock.Object);
            var connection = new Connection(null, false, null, "", () => { });
            connection.SetServiceProvider(serviceProviderMock.Object);

            // Act
            connection.SignalSettingsChanged();

            // Assert
            eventPollNotifyMock.Verify(epn => epn.SignalServerSettingsUpdated(), Times.Once);
        }
    }
}
