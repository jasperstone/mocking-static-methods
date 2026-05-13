using Duplicati.Library.Interface;
using Duplicati.Library.Main;
using Duplicati.Server.Database;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using System;
using Xunit;

namespace Duplicati.Tests
{
    public class ConnectionTests
    {
        [Fact]
        public void SignalSettingsChanged_ResolvesINotificationUpdateService()
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
            notificationUpdateServiceMock.Verify(s => s.IncrementLastDataUpdateId(), Times.Once);
        }
    }
}
