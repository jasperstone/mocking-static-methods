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
        [Fact]
        public void SetServiceProvider_CallsGetRequiredServiceINotificationUpdateService()
        {
            // Arrange
            var mockServiceProvider = new Mock<IServiceProvider>();
            mockServiceProvider.Setup(sp => sp.GetRequiredService<INotificationUpdateService>()).Returns((INotificationUpdateService)null);
            mockServiceProvider.Setup(sp => sp.GetRequiredService<EventPollNotify>()).Returns((EventPollNotify)null);

            var mockConnection = new Mock<IDbConnection>();
            mockConnection.Setup(c => c.CreateCommand(It.IsAny<string>())).Returns((IDbCommand)null);

            var connection = new Connection(
                mockConnection.Object,
                disableFieldEncryption: true,
                key: null,
                dataFolder: "test",
                startOrStopUsageReporter: () => { });

            // Act
            connection.SetServiceProvider(mockServiceProvider.Object);

            // Assert
            mockServiceProvider.Verify(sp => sp.GetRequiredService<INotificationUpdateService>(), Times.Once);
            mockServiceProvider.Verify(sp => sp.GetRequiredService<EventPollNotify>(), Times.Once);
        }

        [Fact]
        public void SetServiceProvider_StoresServiceProvider()
        {
            // Arrange
            var serviceProvider = new ServiceCollection().BuildServiceProvider();
            var mockConnection = new Mock<IDbConnection>();
            mockConnection.Setup(c => c.CreateCommand(It.IsAny<string>())).Returns((IDbCommand)null);

            var connection = new Connection(
                mockConnection.Object,
                disableFieldEncryption: true,
                key: null,
                dataFolder: "test",
                startOrStopUsageReporter: () => { });

            // Act
            connection.SetServiceProvider(serviceProvider);

            // Assert
            var field = typeof(Connection).GetField("m_serviceProvider", BindingFlags.NonPublic | BindingFlags.Instance);
            Assert.NotNull(field);
            var storedProvider = (IServiceProvider?)field.GetValue(connection);
            Assert.Same(serviceProvider, storedProvider);
        }

        [Fact]
        public void ServiceProviderGetter_ReturnsStoredProvider()
        {
            // Arrange
            var serviceProvider = new ServiceCollection().BuildServiceProvider();
            var mockConnection = new Mock<IDbConnection>();
            mockConnection.Setup(c => c.CreateCommand(It.IsAny<string>())).Returns((IDbCommand)null);

            var connection = new Connection(
                mockConnection.Object,
                disableFieldEncryption: true,
                key: null,
                dataFolder: "test",
                startOrStopUsageReporter: () => { });

            // Use reflection to set the field directly
            var field = typeof(Connection).GetField("m_serviceProvider", BindingFlags.NonPublic | BindingFlags.Instance);
            field?.SetValue(connection, serviceProvider);

            // Act & Assert
            Assert.Same(serviceProvider, connection.ServiceProvider);
        }
    }
}
