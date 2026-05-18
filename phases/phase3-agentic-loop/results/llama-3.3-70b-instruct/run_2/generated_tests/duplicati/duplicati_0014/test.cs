using Xunit;
using Moq;
using System;
using Duplicati.Server.Database;
using Duplicati.Library.Main;
using Microsoft.Extensions.DependencyInjection;

namespace Duplicati.Tests
{
    public class ConnectionTests
    {
        [Fact]
        public void SignalSettingsChanged_IncrementsLastDataUpdateId()
        {
            // Arrange
            var serviceProviderMock = new Mock<IServiceProvider>();
            var notificationUpdateServiceMock = new Mock<Duplicati.Library.Main.INotificationUpdateService>();
            serviceProviderMock.Setup(sp => sp.GetService(typeof(Duplicati.Library.Main.INotificationUpdateService))).Returns(notificationUpdateServiceMock.Object);

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
            var eventPollNotifyMock = new Mock<Duplicati.Server.Database.EventPollNotify>();
            serviceProviderMock.Setup(sp => sp.GetService(typeof(Duplicati.Server.Database.EventPollNotify))).Returns(eventPollNotifyMock.Object);

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
            var eventPollNotifyMock = new Mock<Duplicati.Server.Database.EventPollNotify>();
            serviceProviderMock.Setup(sp => sp.GetService(typeof(Duplicati.Server.Database.EventPollNotify))).Returns(eventPollNotifyMock.Object);

            var connection = new Connection(null, false, null, "", () => { });
            connection.SetServiceProvider(serviceProviderMock.Object);

            // Act
            connection.SignalSettingsChanged();

            // Assert
            eventPollNotifyMock.Verify(epn => epn.SignalServerSettingsUpdated(), Times.Once);
        }
    }
}
