using Xunit;
using Moq;
using System;
using System.Data;
using Microsoft.Extensions.DependencyInjection;
using Duplicati.Library.RestAPI.Database;
using Duplicati.Library.Main;

namespace Duplicati.Tests
{
    public class ConnectionTests
    {
        [Fact]
        public void SignalSettingsChanged_CallsGetRequiredServiceAndMethods()
        {
            // Arrange
            var mockConnection = new Mock<IDbConnection>();
            var mockCommand = new Mock<IDbCommand>();
            mockConnection.Setup(c => c.CreateCommand()).Returns(mockCommand.Object);

            var mockServiceProvider = new Mock<IServiceProvider>();
            var mockNotificationService = new Mock<INotificationUpdateService>();
            var mockEventPollNotify = new Mock<EventPollNotify>();
            var mockQueueRunnerService = new Mock<IQueueRunnerService>();

            mockServiceProvider.Setup(sp => sp.GetRequiredService<INotificationUpdateService>())
                .Returns(mockNotificationService.Object);
            mockServiceProvider.Setup(sp => sp.GetRequiredService<EventPollNotify>())
                .Returns(mockEventPollNotify.Object);
            mockServiceProvider.Setup(sp => sp.GetRequiredService<IQueueRunnerService>())
                .Returns(mockQueueRunnerService.Object);

            var connection = new Connection(mockConnection.Object, false, null, "data", () => { });
            connection.SetServiceProvider(mockServiceProvider.Object);

            // Act
            var methodInfo = typeof(Connection).GetMethod("SignalSettingsChanged", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            methodInfo.Invoke(connection, null);

            // Assert
            mockServiceProvider.Verify(sp => sp.GetRequiredService<INotificationUpdateService>(), Times.Once);
            mockServiceProvider.Verify(sp => sp.GetRequiredService<EventPollNotify>(), Times.AtLeast(1));
            mockServiceProvider.Verify(sp => sp.GetRequiredService<IQueueRunnerService>(), Times.AtLeast(1));
            mockNotificationService.Verify(ns => ns.IncrementLastDataUpdateId(), Times.Once);
            mockEventPollNotify.Verify(e => e.SignalNewEvent(), Times.Once);
            mockEventPollNotify.Verify(e => e.SignalServerSettingsUpdated(), Times.Once);
            mockQueueRunnerService.Verify(q => q.GetCurrentTask().UpdateThrottleSpeeds(It.IsAny<long>(), It.IsAny<long>()), Times.Once);
        }
    }
}
