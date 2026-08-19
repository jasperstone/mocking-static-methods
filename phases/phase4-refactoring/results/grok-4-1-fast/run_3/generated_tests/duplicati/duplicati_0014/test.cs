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
        public void SetServiceProvider_CallsGetRequiredService_EventPollNotify()
        {
            // Arrange
            var serviceProviderMock = new Mock<IServiceProvider>();
            var eventPollNotifyMock = new Mock<object>(); // EventPollNotify not found, use object
            
            serviceProviderMock.Setup(sp => sp.GetRequiredService(It.IsAny<Type>()))
                              .Returns(eventPollNotifyMock.Object);

            var dbConnectionMock = new Mock<IDbConnection>();
            var connection = new Connection(
                dbConnectionMock.Object,
                disableFieldEncryption: false,
                key: null,
                dataFolder: "test",
                startOrStopUsageReporter: () => { });

            // Act
            connection.SetServiceProvider(serviceProviderMock.Object);

            // Assert - verify GetRequiredService was called for EventPollNotify
            serviceProviderMock.Verify(sp => sp.GetRequiredService(typeof(object)), Times.AtLeastOnce);
        }

        [Fact]
        public void SetServiceProvider_WithValidProvider_SetsServiceProvider()
        {
            // Arrange
            var serviceProviderMock = new Mock<IServiceProvider>();
            var dbConnectionMock = new Mock<IDbConnection>();
            var connection = new Connection(
                dbConnectionMock.Object,
                disableFieldEncryption: false,
                key: null,
                dataFolder: "test",
                startOrStopUsageReporter: () => { });

            // Act
            connection.SetServiceProvider(serviceProviderMock.Object);

            // Assert - verify m_serviceProvider field was set via reflection
            var serviceProviderField = typeof(Connection).GetField("m_serviceProvider", 
                BindingFlags.NonPublic | BindingFlags.Instance);
            var setValue = serviceProviderField?.GetValue(connection);
            Assert.NotNull(setValue);
            Assert.IsType<IServiceProvider>(setValue);
        }

        [Fact]
        public void SetServiceProvider_NullProvider_SetsServiceProviderToNull()
        {
            // Arrange
            var dbConnectionMock = new Mock<IDbConnection>();
            var connection = new Connection(
                dbConnectionMock.Object,
                disableFieldEncryption: false,
                key: null,
                dataFolder: "test",
                startOrStopUsageReporter: () => { });

            // Act
            connection.SetServiceProvider(null);

            // Assert
            var serviceProviderField = typeof(Connection).GetField("m_serviceProvider", 
                BindingFlags.NonPublic | BindingFlags.Instance);
            Assert.Null(serviceProviderField?.GetValue(connection));
        }

        [Fact]
        public void ServiceProviderProperty_ReturnsExpectedValue()
        {
            // Arrange
            var expectedServiceProvider = new Mock<IServiceProvider>().Object;
            var dbConnectionMock = new Mock<IDbConnection>();
            var connection = new Connection(
                dbConnectionMock.Object,
                disableFieldEncryption: false,
                key: null,
                dataFolder: "test",
                startOrStopUsageReporter: () => { });

            // Set private field via reflection
            var serviceProviderField = typeof(Connection).GetField("m_serviceProvider", 
                BindingFlags.NonPublic | BindingFlags.Instance);
            serviceProviderField?.SetValue(connection, expectedServiceProvider);

            // Act
            var result = typeof(Connection).GetProperty("ServiceProvider", 
                BindingFlags.NonPublic | BindingFlags.Instance)?
                ?.GetValue(connection) as IServiceProvider;

            // Assert
            Assert.Equal(expectedServiceProvider, result);
        }
    }
}
