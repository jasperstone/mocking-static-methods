using System;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;
using Duplicati.Server.Database;

namespace Duplicati.Tests.Server.Database
{
    public class ConnectionTests
    {
        [Fact]
        public void SignalSettingsChanged_CallsExpectedServiceMethods()
        {
            // Arrange
            var mockNotificationUpdateService = new Mock<INotificationUpdateService>();
            var mockEventPollNotify = new Mock<EventPollNotify>();
            var mockQueueRunnerService = new Mock<IQueueRunnerService>();
            var mockLiveControls = new Mock<LiveControls>();
            var mockQueueTask = new Mock<IQueueTask>();

            mockQueueRunnerService.Setup(q => q.GetCurrentTask()).Returns(mockQueueTask.Object);

            var mockServiceProvider = new Mock<IServiceProvider>();
            mockServiceProvider.Setup(sp => sp.GetRequiredService(typeof(INotificationUpdateService)))
                .Returns(mockNotificationUpdateService.Object);
            mockServiceProvider.Setup(sp => sp.GetRequiredService(typeof(EventPollNotify)))
                .Returns(mockEventPollNotify.Object);
            mockServiceProvider.Setup(sp => sp.GetRequiredService(typeof(IQueueRunnerService)))
                .Returns(mockQueueRunnerService.Object);
            mockServiceProvider.Setup(sp => sp.GetRequiredService(typeof(LiveControls)))
                .Returns(mockLiveControls.Object);

            var mockDbConnection = new Mock<System.Data.IDbConnection>();
            var mockDbCommand = new Mock<System.Data.IDbCommand>();
            mockDbConnection.Setup(c => c.CreateCommand(It.IsAny<string>())).Returns(mockDbCommand.Object);

            bool usageReporterInvoked = false;
            Action usageReporter = () => usageReporterInvoked = true;

            var connection = new Connection(mockDbConnection.Object, disableFieldEncryption: true, key: null, dataFolder: "", startOrStopUsageReporter: usageReporter);
            connection.SetServiceProvider(mockServiceProvider.Object);

            // Act
            // Use reflection to invoke private method SignalSettingsChanged
            var method = typeof(Connection).GetMethod("SignalSettingsChanged", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            Assert.NotNull(method);
            method.Invoke(connection, null);

            // Assert
            mockNotificationUpdateService.Verify(n => n.IncrementLastDataUpdateId(), Times.Never()); // Not called in SignalSettingsChanged
            mockEventPollNotify.Verify(e => e.SignalNewEvent(), Times.Once);
            mockEventPollNotify.Verify(e => e.SignalServerSettingsUpdated(), Times.Once);
            mockQueueRunnerService.Verify(q => q.GetCurrentTask(), Times.Once);
            mockQueueTask.Verify(t => t.UpdateThrottleSpeeds(It.IsAny<int>(), It.IsAny<int>()), Times.Once);
            mockLiveControls.Verify(l => l.UpdatePowerModeProvider(), Times.Once);
            Assert.True(usageReporterInvoked);
        }
    }

    // Dummy interfaces and classes to satisfy references in the test
    public interface INotificationUpdateService
    {
        void IncrementLastDataUpdateId();
    }

    public class EventPollNotify
    {
        public virtual void SignalNewEvent() { }
        public virtual void SignalServerSettingsUpdated() { }
    }

    public interface IQueueRunnerService
    {
        IQueueTask? GetCurrentTask();
    }

    public interface IQueueTask
    {
        void UpdateThrottleSpeeds(int uploadSpeedLimit, int downloadSpeedLimit);
    }

    public class LiveControls
    {
        public virtual void UpdatePowerModeProvider() { }
    }
}
