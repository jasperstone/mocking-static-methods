using Xunit;
using Moq;
using System;
using System.Collections.Generic;
using System.Data;
using Microsoft.Extensions.DependencyInjection;
using Duplicati.Library.RestAPI.Database;
using Duplicati.Library.AutoUpdater; // For IQueueRunnerService
using Duplicati.WebserverCore.Abstractions; // For EventPollNotify
using Duplicati.Server.Serialization.Interface; // For INotificationUpdateService

namespace Duplicati.Tests
{
    public class ConnectionTests
    {
        [Fact]
        public void SignalSettingsChanged_CallsRequiredServices()
        {
            // Arrange
            var mockServiceProvider = new Mock<IServiceProvider>();
            var mockNotificationService = new Mock<INotificationUpdateService>();
            var mockEventPollNotify = new Mock<EventPollNotify>();
            var mockQueueRunnerService = new Mock<IQueueRunnerService>();
            var mockCurrentTask = new Mock<IQueueTask>();
            mockQueueRunnerService.Setup(q => q.GetCurrentTask()).Returns(mockCurrentTask.Object);

            mockServiceProvider.Setup(sp => sp.GetRequiredService<INotificationUpdateService>())
                .Returns(mockNotificationService.Object);
            mockServiceProvider.Setup(sp => sp.GetRequiredService<EventPollNotify>())
                .Returns(mockEventPollNotify.Object);
            mockServiceProvider.Setup(sp => sp.GetRequiredService<IQueueRunnerService>())
                .Returns(mockQueueRunnerService.Object);

            var mockConnection = new Mock<IDbConnection>();
            var mockCommand = new Mock<IDbCommand>();
            mockConnection.Setup(c => c.CreateCommand()).Returns(mockCommand.Object);

            var connection = new Connection(
                mockConnection.Object,
                disableFieldEncryption: false,
                key: null,
                dataFolder: "dummyFolder",
                startOrStopUsageReporter: () => { }
            );

            // Set the service provider
            connection.SetServiceProvider(mockServiceProvider.Object);

            // Act
            // Call the private method via reflection
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
