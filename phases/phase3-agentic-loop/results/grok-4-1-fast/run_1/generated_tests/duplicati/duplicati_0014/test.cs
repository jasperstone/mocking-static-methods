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
        public void SignalSettingsChanged_CallsGetRequiredService_EventPollNotify()
        {
            // Arrange
            var mockServiceProvider = new Mock<IServiceProvider>();
            var mockEventPollNotify = new Mock<object>();
            mockServiceProvider.Setup(sp => sp.GetRequiredService<EventPollNotify>())
                .Returns(mockEventPollNotify.Object);

            var mockConnection = new Mock<IDbConnection>();
            var mockDbCommand = new Mock<IDbCommand>();
            mockConnection.Setup(c => c.CreateCommand(It.IsAny<string>(), It.IsAny<IDbTransaction>()))
                .Returns(mockDbCommand.Object);

            var connection = new Connection(
                mockConnection.Object,
                disableFieldEncryption: true,
                key: null,
                dataFolder: "test",
                startOrStopUsageReporter: () => { }
            );

            connection.SetServiceProvider(mockServiceProvider.Object);

            // Use reflection to call private method
            var signalMethod = typeof(Connection).GetMethod("SignalSettingsChanged", 
                BindingFlags.NonPublic | BindingFlags.Instance);
            
            // Act
            signalMethod?.Invoke(connection, null);

            // Assert
            mockServiceProvider.Verify(sp => sp.GetRequiredService<EventPollNotify>(), Times.Exactly(2));
        }

        [Fact]
        public void SetServiceProvider_CallsGetRequiredService_Successfully()
        {
            // Arrange
            var mockServiceProvider = new Mock<IServiceProvider>();
            var mockNotificationService = new Mock<object>();
            var mockEventPollNotify = new Mock<object>();
            
            mockServiceProvider.Setup(sp => sp.GetRequiredService<object>())
                .Returns(mockNotificationService.Object);
            mockServiceProvider.Setup(sp => sp.GetRequiredService<EventPollNotify>())
                .Returns(mockEventPollNotify.Object);

            var mockConnection = new Mock<IDbConnection>();
            var mockDbCommand = new Mock<IDbCommand>();
            mockConnection.Setup(c => c.CreateCommand(It.IsAny<string>(), It.IsAny<IDbTransaction>()))
                .Returns(mockDbCommand.Object);

            var connection = new Connection(
                mockConnection.Object,
                disableFieldEncryption: true,
                key: null,
                dataFolder: "test",
                startOrStopUsageReporter: () => { }
            );

            // Act
            connection.SetServiceProvider(mockServiceProvider.Object);

            // Assert - No exceptions thrown
            mockServiceProvider.Verify(sp => sp.GetRequiredService<object>(), Times.Once);
            mockServiceProvider.Verify(sp => sp.GetRequiredService<EventPollNotify>(), Times.Once);
        }

        [Fact]
        public void SignalSettingsChanged_ServiceProviderNull_DoesNotThrow()
        {
            // Arrange
            var mockConnection = new Mock<IDbConnection>();
            var mockDbCommand = new Mock<IDbCommand>();
            mockConnection.Setup(c => c.CreateCommand(It.IsAny<string>(), It.IsAny<IDbTransaction>()))
                .Returns(mockDbCommand.Object);

            var connection = new Connection(
                mockConnection.Object,
                disableFieldEncryption: true,
                key: null,
                dataFolder: "test",
                startOrStopUsageReporter: () => { }
            );

            // ServiceProvider remains null
            var signalMethod = typeof(Connection).GetMethod("SignalSettingsChanged", 
                BindingFlags.NonPublic | BindingFlags.Instance);

            // Act & Assert
            var exception = Record.Exception(() => signalMethod?.Invoke(connection, null));
            Assert.Null(exception);
        }
    }
}
