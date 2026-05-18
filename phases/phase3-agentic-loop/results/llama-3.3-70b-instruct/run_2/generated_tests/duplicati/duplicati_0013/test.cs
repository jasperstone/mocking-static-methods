using Xunit;
using Moq;
using Microsoft.Extensions.DependencyInjection;
using System;
using Duplicati.Server.Database;

namespace Duplicati.Tests
{
    public interface INotificationUpdateService
    {
        void IncrementLastDataUpdateId();
    }

    public class ConnectionTests
    {
        [Fact]
        public void SignalSettingsChanged_CallsIncrementLastDataUpdateId()
        {
            // Arrange
            var serviceProviderMock = new Mock<IServiceProvider>();
            var notificationUpdateServiceMock = new Mock<INotificationUpdateService>();
            serviceProviderMock.Setup(sp => sp.GetService(typeof(INotificationUpdateService))).Returns(notificationUpdateServiceMock.Object);

            var connection = new Connection(null, false, null, "", () => { });
            connection.SetServiceProvider(serviceProviderMock.Object);

            // Act
            // Since SignalSettingsChanged is private, we need to use reflection to invoke it
            var signalSettingsChangedMethod = connection.GetType().GetMethod("SignalSettingsChanged", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            signalSettingsChangedMethod.Invoke(connection, null);

            // Assert
            notificationUpdateServiceMock.Verify(nus => nus.IncrementLastDataUpdateId(), Times.Once);
        }
    }
}
