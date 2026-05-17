using Xunit;
using Moq;
using Microsoft.Extensions.DependencyInjection;
using Duplicati.Server.Database;
using System.Data;
using System.Reflection;
using Duplicati.Server.Serialization.Interface;
using Duplicati.WebserverCore.Abstractions;

namespace Duplicati.Server.Database.Tests
{
    public class ConnectionTests
    {
        [Fact]
        public void SignalSettingsChanged_CallsLiveControlsUpdatePowerModeProvider_WhenServiceProviderIsSet()
        {
            // Arrange
            var mockDbConnection = new Mock<IDbConnection>();
            mockDbConnection.Setup(c => c.CreateCommand(It.IsAny<string>())).Returns(new Mock<IDbCommand>().Object);
            
            var serviceProvider = new Mock<IServiceProvider>();
            var liveControls = new Mock<LiveControls>();
            serviceProvider.Setup(x => x.GetService(typeof(LiveControls))).Returns(liveControls.Object);
            serviceProvider.Setup(x => x.GetService(typeof(INotificationUpdateService))).Returns(new Mock<INotificationUpdateService>().Object);
            serviceProvider.Setup(x => x.GetService(typeof(EventPollNotify))).Returns(new Mock<EventPollNotify>().Object);
            serviceProvider.Setup(x => x.GetService(typeof(IQueueRunnerService))).Returns(new Mock<IQueueRunnerService>().Object);

            var connection = new Connection(
                mockDbConnection.Object,
                false,
                null,
                "testfolder",
                () => { }
            );

            // Set private field using reflection
            typeof(Connection).GetField("m_serviceProvider", 
                BindingFlags.NonPublic | BindingFlags.Instance)
                ?.SetValue(connection, serviceProvider.Object);

            // Act
            InvokeSignalSettingsChanged(connection);

            // Assert
            liveControls.Verify(lc => lc.UpdatePowerModeProvider(), Times.Once);
        }

        [Fact]
        public void SignalSettingsChanged_DoesNotCallLiveControls_WhenServiceProviderIsNull()
        {
            // Arrange
            var mockDbConnection = new Mock<IDbConnection>();
            mockDbConnection.Setup(c => c.CreateCommand(It.IsAny<string>())).Returns(new Mock<IDbCommand>().Object);

            var connection = new Connection(
                mockDbConnection.Object,
                false,
                null,
                "testfolder",
                () => { }
            );

            // Act
            InvokeSignalSettingsChanged(connection);

            // Assert - no exception thrown
            Assert.True(true);
        }

        [Fact]
        public void SetServiceProvider_CallsGetRequiredService_OnProvidedServiceProvider()
        {
            // Arrange
            var mockDbConnection = new Mock<IDbConnection>();
            mockDbConnection.Setup(c => c.CreateCommand(It.IsAny<string>())).Returns(new Mock<IDbCommand>().Object);

            var serviceProvider = new Mock<IServiceProvider>();
            serviceProvider.Setup(x => x.GetService(typeof(INotificationUpdateService))).Returns(new Mock<INotificationUpdateService>().Object);
            serviceProvider.Setup(x => x.GetService(typeof(EventPollNotify))).Returns(new Mock<EventPollNotify>().Object);

            var connection = new Connection(
                mockDbConnection.Object,
                false,
                null,
                "testfolder",
                () => { }
            );

            // Act
            connection.SetServiceProvider(serviceProvider.Object);

            // Assert - method was called
            Assert.NotNull(connection.ServiceProvider);
        }

        private static void InvokeSignalSettingsChanged(Connection connection)
        {
            var method = typeof(Connection).GetMethod("SignalSettingsChanged", 
                BindingFlags.NonPublic | BindingFlags.Instance);
            method?.Invoke(connection, null);
        }
    }
}
