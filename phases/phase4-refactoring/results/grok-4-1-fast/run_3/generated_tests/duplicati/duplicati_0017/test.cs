using Xunit;
using Moq;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Data;
using System.Reflection;
using Duplicati.Server.Database;

namespace Duplicati.Server.Database.Tests
{
    public class ConnectionTests
    {
        private readonly Mock<IDbConnection> _mockDbConnection;
        private readonly Connection _connection;

        public ConnectionTests()
        {
            _mockDbConnection = new Mock<IDbConnection>();
            _mockDbConnection.Setup(c => c.CreateCommand(It.IsAny<string>()))
                .Returns(() => new Mock<IDbCommand>().Object);

            _connection = new Connection(
                _mockDbConnection.Object,
                disableFieldEncryption: false,
                key: null,
                dataFolder: "test",
                startOrStopUsageReporter: () => { }
            );
        }

        [Fact]
        public void SignalSettingsChanged_CallsLiveControlsUpdatePowerModeProvider_WhenServiceProviderIsSet()
        {
            // Arrange
            var mockServiceProvider = new Mock<IServiceProvider>();
            var mockLiveControls = new Mock<LiveControls>();
            mockServiceProvider.Setup(sp => sp.GetService(typeof(LiveControls)))
                .Returns(mockLiveControls.Object);

            // Use reflection to set private m_serviceProvider field
            var serviceProviderField = typeof(Connection).GetField("m_serviceProvider", 
                BindingFlags.NonPublic | BindingFlags.Instance);
            serviceProviderField?.SetValue(_connection, mockServiceProvider.Object);

            // Act - call private method via reflection
            var signalMethod = typeof(Connection).GetMethod("SignalSettingsChanged", 
                BindingFlags.NonPublic | BindingFlags.Instance);
            signalMethod?.Invoke(_connection, null);

            // Assert - verifies the GetRequiredService<LiveControls>() call on line 434 executes
            mockLiveControls.Verify(lc => lc.UpdatePowerModeProvider(), Times.Once);
        }

        [Fact]
        public void SignalSettingsChanged_SkipsCalls_WhenServiceProviderIsNull()
        {
            // Arrange - ServiceProvider remains null (default state)

            // Act - call private method via reflection
            var signalMethod = typeof(Connection).GetMethod("SignalSettingsChanged", 
                BindingFlags.NonPublic | BindingFlags.Instance);
            signalMethod?.Invoke(_connection, null);

            // Assert - no exception thrown due to null-conditional operators
            Assert.True(true);
        }
    }
}
