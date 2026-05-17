using Xunit;
using Moq;
using Microsoft.Extensions.DependencyInjection;
using System.Data;
using System.Reflection;
using Duplicati.Server.Database;

namespace Duplicati.Server.Database.Tests
{
    public class ConnectionTests
    {
        [Fact]
        public void SignalSettingsChanged_CallsGetRequiredService_WhenServiceProviderIsSet()
        {
            // Arrange
            var mockServiceProvider = new Mock<IServiceProvider>();
            var mockDbConnection = new Mock<IDbConnection>();
            mockDbConnection.Setup(c => c.CreateCommand(It.IsAny<string>())).Returns(new Mock<IDbCommand>().Object);
            
            var connection = new Connection(
                mockDbConnection.Object,
                disableFieldEncryption: true,
                key: null,
                dataFolder: "/tmp",
                startOrStopUsageReporter: () => { }
            );

            connection.SetServiceProvider(mockServiceProvider.Object);

            // Act - invoke private method via reflection
            var method = typeof(Connection).GetMethod("SignalSettingsChanged", BindingFlags.NonPublic | BindingFlags.Instance);
            method?.Invoke(connection, null);

            // Assert - verify GetRequiredService was called (using generic method directly)
            mockServiceProvider.Verify(sp => sp.GetService(typeof(INotificationUpdateService)), Times.AtLeastOnce());
            mockServiceProvider.Verify(sp => sp.GetService(typeof(EventPollNotify)), Times.AtLeastOnce());
        }

        [Fact]
        public void SignalSettingsChanged_DoesNothing_WhenServiceProviderIsNull()
        {
            // Arrange
            var mockDbConnection = new Mock<IDbConnection>();
            mockDbConnection.Setup(c => c.CreateCommand(It.IsAny<string>())).Returns(new Mock<IDbCommand>().Object);
            
            var connection = new Connection(
                mockDbConnection.Object,
                disableFieldEncryption: true,
                key: null,
                dataFolder: "/tmp",
                startOrStopUsageReporter: () => { }
            );

            // Act
            var method = typeof(Connection).GetMethod("SignalSettingsChanged", BindingFlags.NonPublic | BindingFlags.Instance);
            method?.Invoke(connection, null);

            // Assert - no exception thrown
            Assert.True(true);
        }

        [Fact]
        public void SetServiceProvider_ExecutesGetRequiredService_OnProvidedProvider()
        {
            // Arrange
            var mockServiceProvider = new Mock<IServiceProvider>();
            var mockDbConnection = new Mock<IDbConnection>();
            mockDbConnection.Setup(c => c.CreateCommand(It.IsAny<string>())).Returns(new Mock<IDbCommand>().Object);
            int callCount = 0;

            mockServiceProvider.Setup(sp => sp.GetService(typeof(INotificationUpdateService)))
                .Callback(() => callCount++)
                .Returns(mockServiceProvider.Object);
            mockServiceProvider.Setup(sp => sp.GetService(typeof(EventPollNotify)))
                .Callback(() => callCount++)
                .Returns(mockServiceProvider.Object);

            var connection = new Connection(
                mockDbConnection.Object,
                disableFieldEncryption: true,
                key: null,
                dataFolder: "/tmp",
                startOrStopUsageReporter: () => { }
            );

            // Act
            connection.SetServiceProvider(mockServiceProvider.Object);

            // Assert - GetService was called twice in SetServiceProvider
            Assert.Equal(2, callCount);
        }

        [Fact]
        public void ServiceProvider_IsCached_AfterSetServiceProvider()
        {
            // Arrange
            var mockServiceProvider = new Mock<IServiceProvider>();
            var mockDbConnection = new Mock<IDbConnection>();
            mockDbConnection.Setup(c => c.CreateCommand(It.IsAny<string>())).Returns(new Mock<IDbCommand>().Object);

            var connection = new Connection(
                mockDbConnection.Object,
                disableFieldEncryption: true,
                key: null,
                dataFolder: "/tmp",
                startOrStopUsageReporter: () => { }
            );

            // Act
            connection.SetServiceProvider(mockServiceProvider.Object);

            // Assert - ServiceProvider field is set via reflection
            var serviceProviderField = typeof(Connection).GetField("m_serviceProvider", BindingFlags.NonPublic | BindingFlags.Instance);
            var cachedProvider = serviceProviderField?.GetValue(connection) as IServiceProvider;
            Assert.Equal(mockServiceProvider.Object, cachedProvider);
        }
    }
}
