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
        public void SignalSettingsChanged_WithServiceProvider_CallsEventPollNotifyMethods()
        {
            // Arrange
            var mockEventPollNotify = new Mock<EventPollNotify>();
            var services = new ServiceCollection();
            services.AddSingleton(mockEventPollNotify.Object);
            var serviceProvider = services.BuildServiceProvider();

            var mockDbConnection = Mock.Of<IDbConnection>();
            var connection = new Connection(
                mockDbConnection,
                disableFieldEncryption: false,
                key: null,
                dataFolder: "test-data",
                startOrStopUsageReporter: () => { }
            );

            connection.SetServiceProvider(serviceProvider);

            // Act
            SignalSettingsChangedMethod.Invoke(connection, null);

            // Assert
            mockEventPollNotify.Verify(epn => epn.SignalNewEvent(), Times.Once);
            mockEventPollNotify.Verify(epn => epn.SignalServerSettingsUpdated(), Times.Once);
        }

        [Fact]
        public void SignalSettingsChanged_ServiceProviderNull_DoesNotThrow()
        {
            // Arrange
            var mockDbConnection = Mock.Of<IDbConnection>();
            var connection = new Connection(
                mockDbConnection,
                disableFieldEncryption: false,
                key: null,
                dataFolder: "test-data",
                startOrStopUsageReporter: () => { }
            );

            // Act & Assert
            Assert.DoesNotThrow(() => SignalSettingsChangedMethod.Invoke(connection, null));
        }

        [Fact]
        public void SetServiceProvider_CallsGetRequiredService_SetsInternalFields()
        {
            // Arrange
            var services = new ServiceCollection();
            services.AddSingleton<EventPollNotify>(new Mock<EventPollNotify>().Object);
            var serviceProvider = services.BuildServiceProvider();

            var mockDbConnection = Mock.Of<IDbConnection>();
            var connection = new Connection(
                mockDbConnection,
                disableFieldEncryption: false,
                key: null,
                dataFolder: "test-data",
                startOrStopUsageReporter: () => { }
            );

            // Act
            connection.SetServiceProvider(serviceProvider);

            // Assert - verify via reflection that internal fields are populated
            var eventPollField = typeof(Connection).GetField("m_eventPollNotifyer", BindingFlags.NonPublic | BindingFlags.Instance);
            Assert.NotNull(eventPollField);
            Assert.NotNull(eventPollField.GetValue(connection));
        }
    }
}
