using Xunit;
using Moq;
using System;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Duplicati.Library.RestAPI.Database;

namespace Duplicati.Tests
{
    public class ConnectionTests
    {
        [Fact]
        public void SignalSettingsChanged_CallsGetRequiredServiceAndSignals()
        {
            // Arrange
            var mockEventPollNotify = new Mock<EventPollNotify>();
            var mockServiceProvider = new Mock<IServiceProvider>();
            mockServiceProvider.Setup(sp => sp.GetRequiredService<EventPollNotify>())
                               .Returns(mockEventPollNotify.Object);

            var mockNotificationService = new Mock<INotificationUpdateService>();
            mockServiceProvider.Setup(sp => sp.GetRequiredService<INotificationUpdateService>())
                               .Returns(mockNotificationService.Object);

            var mockQueueRunnerService = new Mock<IQueueRunnerService>();
            var mockLiveControls = new Mock<LiveControls>();

            // Setup for GetRequiredService to return mocks for each service
            mockServiceProvider.Setup(sp => sp.GetRequiredService<IQueueRunnerService>())
                               .Returns(mockQueueRunnerService.Object);
            mockServiceProvider.Setup(sp => sp.GetRequiredService<LiveControls>())
                               .Returns(mockLiveControls.Object);

            var mockConnection = new Mock<IDbConnection>();
            var mockCommand = new Mock<IDbCommand>();
            mockConnection.Setup(c => c.CreateCommand()).Returns(mockCommand.Object);

            var connection = new Connection(
                mockConnection.Object,
                disableFieldEncryption: false,
                key: null,
                dataFolder: "dummy",
                startOrStopUsageReporter: () => { }
            );

            // Set the service provider
            connection.SetServiceProvider(mockServiceProvider.Object);

            // Act
            var method = typeof(Connection).GetMethod("SignalSettingsChanged", BindingFlags.NonPublic | BindingFlags.Instance);
            method.Invoke(connection, null);

            // Assert
            mockEventPollNotify.Verify(e => e.SignalNewEvent(), Times.Once);
            mockEventPollNotify.Verify(e => e.SignalServerSettingsUpdated(), Times.Once);
        }
    }
}
