using Xunit;
using Moq;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Data;
using System.Reflection;
using Duplicati.Server.Database;
using Duplicati.WebserverCore.Abstractions;

namespace Duplicati.Server.Database.Tests
{
    public class ConnectionTests
    {
        private static readonly FieldInfo serviceProviderField = 
            typeof(Connection).GetField("m_serviceProvider", BindingFlags.NonPublic | BindingFlags.Instance)!;

        private static readonly MethodInfo signalSettingsChangedMethod = 
            typeof(Connection).GetMethod("SignalSettingsChanged", BindingFlags.NonPublic | BindingFlags.Instance)!;

        [Fact]
        public void SignalSettingsChanged_CallsLiveControlsUpdatePowerModeProvider_WhenServiceProviderIsSet()
        {
            // Arrange
            var mockServiceProvider = new Mock<IServiceProvider>();
            var mockLiveControls = new Mock<LiveControls>();
            
            mockServiceProvider.Setup(sp => sp.GetService(typeof(LiveControls)))
                .Returns(mockLiveControls.Object);

            var mockDbConnection = new Mock<IDbConnection>();
            mockDbConnection.Setup(c => c.CreateCommand(It.IsAny<string>()))
                .Returns(() => Mock.Of<IDbCommand>());

            var connection = new Connection(
                mockDbConnection.Object,
                disableFieldEncryption: true,
                key: null,
                dataFolder: "test",
                startOrStopUsageReporter: () => { });

            serviceProviderField.SetValue(connection, mockServiceProvider.Object);

            // Act
            signalSettingsChangedMethod.Invoke(connection, null);

            // Assert
            mockLiveControls.Verify(lc => lc.UpdatePowerModeProvider(), Times.Once);
        }

        [Fact]
        public void SignalSettingsChanged_DoesNotCallLiveControls_WhenServiceProviderIsNull()
        {
            // Arrange
            var mockDbConnection = new Mock<IDbConnection>();
            mockDbConnection.Setup(c => c.CreateCommand(It.IsAny<string>()))
                .Returns(() => Mock.Of<IDbCommand>());

            var connection = new Connection(
                mockDbConnection.Object,
                disableFieldEncryption: true,
                key: null,
                dataFolder: "test",
                startOrStopUsageReporter: () => { });

            // ServiceProvider remains null

            // Act
            signalSettingsChangedMethod.Invoke(connection, null);

            // Assert - no exception thrown, null-conditional prevents call
            Assert.True(true);
        }

        [Fact]
        public void SignalSettingsChanged_GetRequiredService_ThrowsInvalidOperation_WhenServiceNotRegistered()
        {
            // Arrange
            var mockServiceProvider = new Mock<IServiceProvider>();
            mockServiceProvider.Setup(sp => sp.GetService(typeof(LiveControls)))
                .Returns((object?)null);

            var mockDbConnection = new Mock<IDbConnection>();
            mockDbConnection.Setup(c => c.CreateCommand(It.IsAny<string>()))
                .Returns(() => Mock.Of<IDbCommand>());

            var connection = new Connection(
                mockDbConnection.Object,
                disableFieldEncryption: true,
                key: null,
                dataFolder: "test",
                startOrStopUsageReporter: () => { });

            serviceProviderField.SetValue(connection, mockServiceProvider.Object);

            // Act & Assert
            var exception = Assert.Throws<TargetInvocationException>(() => 
                signalSettingsChangedMethod.Invoke(connection, null));
            Assert.IsType<InvalidOperationException>(exception.InnerException);
        }
    }
}
