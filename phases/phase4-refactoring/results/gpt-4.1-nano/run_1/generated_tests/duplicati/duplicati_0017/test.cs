using Xunit;
using Moq;
using System;
using System.Data;
using System.Collections.Generic;
using System.Linq;
using Duplicati.Library.RestAPI.Database;
using Microsoft.Extensions.DependencyInjection;

namespace Duplicati.Tests
{
    public class ConnectionTests
    {
        [Fact]
        public void SignalSettingsChanged_CallsRequiredServices()
        {
            // Arrange
            var mockConnection = new Mock<IDbConnection>();
            var mockServiceProvider = new Mock<IServiceProvider>();
            var mockNotificationService = new Mock<INotificationUpdateService>();
            var mockEventPollNotify = new Mock<EventPollNotify>();
            var mockQueueRunnerService = new Mock<IQueueRunnerService>();

            // Setup the service provider to return the mocks
            mockServiceProvider.Setup(sp => sp.GetRequiredService<INotificationUpdateService>())
                .Returns(mockNotificationService.Object);
            mockServiceProvider.Setup(sp => sp.GetRequiredService<EventPollNotify>())
                .Returns(mockEventPollNotify.Object);
            mockServiceProvider.Setup(sp => sp.GetRequiredService<IQueueRunnerService>())
                .Returns(mockQueueRunnerService.Object);

            // Instantiate Connection with dummy data
            var connection = new Connection(
                mockConnection.Object,
                disableFieldEncryption: false,
                key: null,
                dataFolder: "dummyFolder",
                startOrStopUsageReporter: () => { }
            );

            // Inject the mock service provider
            connection.SetServiceProvider(mockServiceProvider.Object);

            // Use reflection to invoke the private method
            var methodInfo = typeof(Connection).GetMethod("SignalSettingsChanged", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            methodInfo.Invoke(connection, null);

            // Assert
            mockNotificationService.Verify(s => s.IncrementLastDataUpdateId(), Times.Once);
            mockEventPollNotify.Verify(e => e.SignalNewEvent(), Times.Once);
            mockEventPollNotify.Verify(e => e.SignalServerSettingsUpdated(), Times.Once);
            mockQueueRunnerService.Verify(q => q.GetCurrentTask().UpdateThrottleSpeeds(It.IsAny<long>(), It.IsAny<long>()), Times.Once);
        }
    }
}
