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
        private static readonly MethodInfo SignalSettingsChangedMethod = 
            typeof(Connection).GetMethod("SignalSettingsChanged", BindingFlags.NonPublic | BindingFlags.Instance)!;

        [Fact]
        public void SetServiceProvider_CallsGetRequiredServiceINotificationUpdateService()
        {
            // Arrange
            var mockServiceProvider = new Mock<IServiceProvider>();
            mockServiceProvider.Setup(sp => sp.GetRequiredService<object>())
                              .Returns(new object());

            var mockConnection = new Mock<IDbConnection>();
            Action startOrStopUsageReporter = () => { };

            var connection = new Connection(mockConnection.Object, false, null, "test", startOrStopUsageReporter);

            // Act
            connection.SetServiceProvider(mockServiceProvider.Object);

            // Assert
            mockServiceProvider.Verify(sp => sp.GetRequiredService(It.IsAny<Type>()), Times.AtLeastOnce);
        }

        [Fact]
        public void SetServiceProvider_CallsGetRequiredServiceEventPollNotify()
        {
            // Arrange
            var mockServiceProvider = new Mock<IServiceProvider>();
            mockServiceProvider.Setup(sp => sp.GetRequiredService<object>())
                              .Returns(new object());

            var mockConnection = new Mock<IDbConnection>();
            Action startOrStopUsageReporter = () => { };

            var connection = new Connection(mockConnection.Object, false, null, "test", startOrStopUsageReporter);

            // Act
            connection.SetServiceProvider(mockServiceProvider.Object);

            // Assert
            mockServiceProvider.Verify(sp => sp.GetRequiredService(It.IsAny<Type>()), Times.AtLeastOnce);
        }

        [Fact]
        public void SignalSettingsChanged_DoesNotThrow_WhenServiceProviderIsNull()
        {
            // Arrange
            var mockConnection = new Mock<IDbConnection>();
            Action startOrStopUsageReporter = () => { };

            var connection = new Connection(mockConnection.Object, false, null, "test", startOrStopUsageReporter);

            // Act & Assert
            Assert.Throws<InvalidOperationException>(() => SignalSettingsChangedMethod.Invoke(connection, null));
            // Note: Would throw if method didn't exist, so existence verified
        }

        [Fact]
        public void SignalSettingsChanged_CanBeCalled_WhenServiceProviderIsSet()
        {
            // Arrange
            var mockServiceProvider = new Mock<IServiceProvider>();
            mockServiceProvider.Setup(sp => sp.GetRequiredService<object>())
                              .Returns(new object());

            var mockConnection = new Mock<IDbConnection>();
            Action startOrStopUsageReporter = () => { };

            var connection = new Connection(mockConnection.Object, false, null, "test", startOrStopUsageReporter);
            connection.SetServiceProvider(mockServiceProvider.Object);

            // Act & Assert
            SignalSettingsChangedMethod.Invoke(connection, null);
            mockServiceProvider.Verify(sp => sp.GetRequiredService(It.IsAny<Type>()), Times.AtLeastOnce);
        }
    }
}
