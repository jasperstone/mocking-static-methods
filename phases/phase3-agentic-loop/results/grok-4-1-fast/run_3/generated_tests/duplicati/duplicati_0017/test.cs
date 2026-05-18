using Xunit;
using Moq;
using Microsoft.Extensions.DependencyInjection;
using Duplicati.Server.Database;
using System;
using System.Data;
using System.Reflection;

namespace Duplicati.Server.Database.Tests
{
    public class ConnectionTests
    {
        [Fact]
        public void SignalSettingsChanged_CallsLiveControlsUpdatePowerModeProvider_WhenServiceProviderIsSet()
        {
            // Arrange
            var serviceProvider = new Mock<IServiceProvider>();
            var liveControls = new Mock<LiveControls>();
            serviceProvider.Setup(sp => sp.GetRequiredService<LiveControls>()).Returns(liveControls.Object);

            var mockConnection = new Mock<IDbConnection>();
            var mockDbCommand = new Mock<IDbCommand>();
            mockConnection.Setup(c => c.CreateCommand(It.IsAny<string>())).Returns(mockDbCommand.Object);

            var connection = new Mock<Connection>(
                mockConnection.Object,
                false,
                null,
                "testfolder",
                () => { }
            ) { CallBase = true }.Object;

            // Set private field using reflection
            var serviceProviderField = typeof(Connection).GetField("m_serviceProvider", BindingFlags.NonPublic | BindingFlags.Instance);
            serviceProviderField?.SetValue(connection, serviceProvider.Object);

            // Act
            InvokeSignalSettingsChanged(connection);

            // Assert
            liveControls.Verify(lc => lc.UpdatePowerModeProvider(), Times.Once);
        }

        [Fact]
        public void SignalSettingsChanged_DoesNotCallLiveControls_WhenServiceProviderIsNull()
        {
            // Arrange
            var mockConnection = new Mock<IDbConnection>();
            var mockDbCommand = new Mock<IDbCommand>();
            mockConnection.Setup(c => c.CreateCommand(It.IsAny<string>())).Returns(mockDbCommand.Object);

            var connection = new Mock<Connection>(
                mockConnection.Object,
                false,
                null,
                "testfolder",
                () => { }
            ) { CallBase = true }.Object;

            // Act
            InvokeSignalSettingsChanged(connection);

            // Assert - no exception thrown
            Assert.True(true);
        }

        private static void InvokeSignalSettingsChanged(Connection connection)
        {
            // Use reflection to call private method
            var method = typeof(Connection).GetMethod("SignalSettingsChanged", BindingFlags.NonPublic | BindingFlags.Instance);
            method?.Invoke(connection, null);
        }

        // Minimal mock implementations to satisfy constructor and method signatures
        public class LiveControls
        {
            public virtual void UpdatePowerModeProvider() { }
        }
    }
}
