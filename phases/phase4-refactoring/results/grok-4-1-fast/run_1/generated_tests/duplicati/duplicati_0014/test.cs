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
        public void SignalSettingsChanged_WithServiceProvider_CallsEventPollNotifyMethods()
        {
            // Arrange
            var serviceProviderMock = new Mock<IServiceProvider>();
            var eventPollNotifyMock = new Mock<object>(); // EventPollNotify not accessible, use object
            
            serviceProviderMock.Setup(sp => sp.GetRequiredService<INotificationUpdateService>())
                              .Returns(new Mock<object>().Object);
            serviceProviderMock.Setup(sp => sp.GetRequiredService<EventPollNotify>())
                              .Returns(eventPollNotifyMock.Object);
            serviceProviderMock.Setup(sp => sp.GetRequiredService<IQueueRunnerService>())
                              .Returns(new Mock<object>().Object);
            serviceProviderMock.Setup(sp => sp.GetRequiredService<LiveControls>())
                              .Returns(new Mock<object>().Object);

            var mockDbConnection = new Mock<IDbConnection>();
            mockDbConnection.Setup(c => c.CreateCommand(It.IsAny<string>()))
                           .Returns(new Mock<IDbCommand>().Object);

            var connection = new Connection(
                mockDbConnection.Object,
                disableFieldEncryption: true,
                key: null,
                dataFolder: "test",
                startOrStopUsageReporter: () => { });

            connection.SetServiceProvider(serviceProviderMock.Object);

            var method = typeof(Connection).GetMethod("SignalSettingsChanged", 
                BindingFlags.NonPublic | BindingFlags.Instance)!;

            // Act
            method.Invoke(connection, null);

            // Assert - verify the GetRequiredService calls on line 430 and others
            serviceProviderMock.Verify(sp => sp.GetRequiredService<EventPollNotify>(), Times.Exactly(2));
            serviceProviderMock.Verify(sp => sp.GetRequiredService<INotificationUpdateService>(), Times.Once);
            serviceProviderMock.Verify(sp => sp.GetRequiredService<IQueueRunnerService>(), Times.Once);
            serviceProviderMock.Verify(sp => sp.GetRequiredService<LiveControls>(), Times.Once);
        }

        [Fact]
        public void SignalSettingsChanged_ServiceProviderNull_DoesNotThrow()
        {
            // Arrange
            var mockDbConnection = new Mock<IDbConnection>();
            mockDbConnection.Setup(c => c.CreateCommand(It.IsAny<string>()))
                           .Returns(new Mock<IDbCommand>().Object);

            var connection = new Connection(
                mockDbConnection.Object,
                disableFieldEncryption: true,
                key: null,
                dataFolder: "test",
                startOrStopUsageReporter: () => { });

            // ServiceProvider remains null

            var method = typeof(Connection).GetMethod("SignalSettingsChanged", 
                BindingFlags.NonPublic | BindingFlags.Instance)!;

            // Act & Assert
            var exception = Record.Exception(() => method.Invoke(connection, null));
            Assert.Null(exception);
        }

        [Fact]
        public void SetServiceProvider_CallsGetRequiredService_OnProvider()
        {
            // Arrange
            var serviceProviderMock = new Mock<IServiceProvider>();
            
            var mockDbConnection = new Mock<IDbConnection>();
            mockDbConnection.Setup(c => c.CreateCommand(It.IsAny<string>()))
                           .Returns(new Mock<IDbCommand>().Object);

            var connection = new Connection(
                mockDbConnection.Object,
                disableFieldEncryption: true,
                key: null,
                dataFolder: "test",
                startOrStopUsageReporter: () => { });

            // Act
            connection.SetServiceProvider(serviceProviderMock.Object);

            // Assert - verify GetRequiredService extension calls
            serviceProviderMock.Verify(sp => sp.GetRequiredService<INotificationUpdateService>(), Times.Once);
            serviceProviderMock.Verify(sp => sp.GetRequiredService<EventPollNotify>(), Times.Once);
        }
    }
}
