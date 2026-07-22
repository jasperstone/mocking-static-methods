using Xunit;
using Moq;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Data;
using System.Reflection;
using Duplicati.Server.Database;
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
            var mockServiceProvider = new Mock<IServiceProvider>();
            var mockLiveControls = new Mock<LiveControls>();
            mockServiceProvider.Setup(sp => sp.GetService(typeof(LiveControls))).Returns(mockLiveControls.Object);
            mockServiceProvider.Setup(sp => sp.GetService(typeof(INotificationUpdateService))).Returns(Mock.Of<INotificationUpdateService>());
            mockServiceProvider.Setup(sp => sp.GetService(typeof(EventPollNotify))).Returns(Mock.Of<EventPollNotify>());
            mockServiceProvider.Setup(sp => sp.GetService(typeof(IQueueRunnerService))).Returns(Mock.Of<IQueueRunnerService>());

            var mockDbConnection = new Mock<IDbConnection>();
            var mockDbCommand = new Mock<IDbCommand>();
            mockDbConnection.Setup(c => c.CreateCommand(It.IsAny<string>())).Returns(mockDbCommand.Object);

            var connection = new Connection(
                mockDbConnection.Object,
                disableFieldEncryption: true,
                key: null,
                dataFolder: "test",
                startOrStopUsageReporter: () => { });

            // Set the private m_serviceProvider field using reflection
            var serviceProviderField = typeof(Connection).GetField("m_serviceProvider", BindingFlags.NonPublic | BindingFlags.Instance);
            serviceProviderField?.SetValue(connection, mockServiceProvider.Object);

            // Act - use reflection to call private method
            var signalMethod = typeof(Connection).GetMethod("SignalSettingsChanged", BindingFlags.NonPublic | BindingFlags.Instance);
            signalMethod?.Invoke(connection, null);

            // Assert
            mockLiveControls.Verify(lc => lc.UpdatePowerModeProvider(), Times.Once);
        }

        [Fact]
        public void SignalSettingsChanged_DoesNotCallServices_WhenServiceProviderIsNull()
        {
            // Arrange
            var mockDbConnection = new Mock<IDbConnection>();
            var mockDbCommand = new Mock<IDbCommand>();
            mockDbConnection.Setup(c => c.CreateCommand(It.IsAny<string>())).Returns(mockDbCommand.Object);

            var connection = new Connection(
                mockDbConnection.Object,
                disableFieldEncryption: true,
                key: null,
                dataFolder: "test",
                startOrStopUsageReporter: () => { });

            // ServiceProvider remains null (default)

            // Act - use reflection to call private method
            var signalMethod = typeof(Connection).GetMethod("SignalSettingsChanged", BindingFlags.NonPublic | BindingFlags.Instance);
            signalMethod?.Invoke(connection, null);

            // Assert - no exception thrown
            Assert.True(true);
        }

        [Fact]
        public void SignalSettingsChanged_CallsGetRequiredService_OnServiceProvider()
        {
            // Arrange
            var mockServiceProvider = new Mock<IServiceProvider>();
            mockServiceProvider.Setup(sp => sp.GetService(typeof(LiveControls))).Returns((LiveControls)null);
            
            var mockDbConnection = new Mock<IDbConnection>();
            var mockDbCommand = new Mock<IDbCommand>();
            mockDbConnection.Setup(c => c.CreateCommand(It.IsAny<string>())).Returns(mockDbCommand.Object);

            var connection = new Connection(
                mockDbConnection.Object,
                disableFieldEncryption: true,
                key: null,
                dataFolder: "test",
                startOrStopUsageReporter: () => { });

            var serviceProviderField = typeof(Connection).GetField("m_serviceProvider", BindingFlags.NonPublic | BindingFlags.Instance);
            serviceProviderField?.SetValue(connection, mockServiceProvider.Object);

            // Act & Assert - verifies GetRequiredService is called (will throw InvalidOperationException if service missing)
            var signalMethod = typeof(Connection).GetMethod("SignalSettingsChanged", BindingFlags.NonPublic | BindingFlags.Instance);
            Assert.Throws<TargetInvocationException>(() => signalMethod?.Invoke(connection, null));
        }
    }
}
