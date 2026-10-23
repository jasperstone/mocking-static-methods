using System;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;
using Duplicati.Server.Database;
using Duplicati.Library.RestAPI;
using Duplicati.Server;

namespace Duplicati.Tests.Server.Database
{
    public class ConnectionTests
    {
        [Fact]
        public void SignalSettingsChanged_CallsExpectedServices()
        {
            // Arrange
            var mockNotificationUpdateService = new Mock<INotificationUpdateService>();
            var mockEventPollNotify = new Mock<EventPollNotify>();
            var mockQueueRunnerService = new Mock<IQueueRunnerService>();
            var mockLiveControls = new Mock<LiveControls>(MockBehavior.Loose, null as Connection);
            var mockQueueRunnerTask = new Mock<IQueueRunnerTask>();

            mockQueueRunnerService.Setup(q => q.GetCurrentTask()).Returns(mockQueueRunnerTask.Object);

            var mockServiceProvider = new Mock<IServiceProvider>();
            mockServiceProvider.Setup(sp => sp.GetRequiredService<INotificationUpdateService>()).Returns(mockNotificationUpdateService.Object);
            mockServiceProvider.Setup(sp => sp.GetRequiredService<EventPollNotify>()).Returns(mockEventPollNotify.Object);
            mockServiceProvider.Setup(sp => sp.GetRequiredService<IQueueRunnerService>()).Returns(mockQueueRunnerService.Object);
            mockServiceProvider.Setup(sp => sp.GetRequiredService<LiveControls>()).Returns(mockLiveControls.Object);

            var connection = new Connection(
                connection: new Mock<System.Data.IDbConnection>().Object,
                disableFieldEncryption: true,
                key: null,
                dataFolder: "",
                startOrStopUsageReporter: () => { }
            );

            connection.SetServiceProvider(mockServiceProvider.Object);

            // Act
            var method = typeof(Connection).GetMethod("SignalSettingsChanged", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            Assert.NotNull(method);
            method.Invoke(connection, null);

            // Assert
            mockNotificationUpdateService.Verify(s => s.IncrementLastDataUpdateId(), Times.Once);
            mockEventPollNotify.Verify(s => s.SignalNewEvent(), Times.Once);
            mockEventPollNotify.Verify(s => s.SignalServerSettingsUpdated(), Times.Once);
            mockQueueRunnerService.Verify(s => s.GetCurrentTask(), Times.Once);
            mockQueueRunnerTask.Verify(t => t.UpdateThrottleSpeeds(It.IsAny<int>(), It.IsAny<int>()), Times.Once);
            mockLiveControls.Verify(l => l.UpdatePowerModeProvider(), Times.Once);
        }
    }

    // Minimal interfaces to satisfy compilation for missing types
    public interface IQueueRunnerService
    {
        IQueueRunnerTask? GetCurrentTask();
    }

    public interface IQueueRunnerTask
    {
        void UpdateThrottleSpeeds(int uploadSpeedLimit, int downloadSpeedLimit);
    }
}
