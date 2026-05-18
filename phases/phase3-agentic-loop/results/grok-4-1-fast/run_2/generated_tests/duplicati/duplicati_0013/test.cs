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
        public void SignalSettingsChanged_CallsGetRequiredService_WhenServiceProviderIsSet()
        {
            // Arrange
            var mockServiceProvider = new Mock<IServiceProvider>();
            mockServiceProvider.Setup(sp => sp.GetRequiredService(It.IsAny<Type>())).Returns(new object());
            
            var mockDbConnection = new Mock<IDbConnection>().Object;
            
            var connection = new Connection(
                mockDbConnection, 
                disableFieldEncryption: false, 
                key: null, 
                dataFolder: "test", 
                startOrStopUsageReporter: () => { }
            );
            
            connection.SetServiceProvider(mockServiceProvider.Object);
            
            var signalMethod = typeof(Connection).GetMethod("SignalSettingsChanged", BindingFlags.NonPublic | BindingFlags.Instance);
            
            // Act
            signalMethod!.Invoke(connection, null);
            
            // Assert - Verifies the null-safe call pattern covers line 429
            mockServiceProvider.Verify(sp => sp.GetRequiredService(It.IsAny<Type>()), Times.AtLeastOnce);
        }

        [Fact]
        public void SignalSettingsChanged_DoesNotThrow_WhenServiceProviderIsNull()
        {
            // Arrange
            var mockDbConnection = new Mock<IDbConnection>().Object;
            
            var connection = new Connection(
                mockDbConnection, 
                disableFieldEncryption: false, 
                key: null, 
                dataFolder: "test", 
                startOrStopUsageReporter: () => { }
            );
            
            var signalMethod = typeof(Connection).GetMethod("SignalSettingsChanged", BindingFlags.NonPublic | BindingFlags.Instance);
            
            // Act & Assert
            var exception = Record.Exception(() => signalMethod!.Invoke(connection, null));
            Assert.Null(exception);
        }

        [Fact]
        public void SetServiceProvider_CallsGetRequiredService()
        {
            // Arrange
            var mockServiceProvider = new Mock<IServiceProvider>();
            mockServiceProvider.Setup(sp => sp.GetRequiredService(It.IsAny<Type>())).Returns(new object());
            
            var mockDbConnection = new Mock<IDbConnection>().Object;
            
            var connection = new Connection(
                mockDbConnection, 
                disableFieldEncryption: false, 
                key: null, 
                dataFolder: "test", 
                startOrStopUsageReporter: () => { }
            );
            
            // Act
            connection.SetServiceProvider(mockServiceProvider.Object);
            
            // Assert - Verifies GetRequiredService calls in SetServiceProvider
            mockServiceProvider.Verify(sp => sp.GetRequiredService(It.IsAny<Type>()), Times.Exactly(2));
        }
    }
}
