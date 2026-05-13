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
        public void SignalSettingsChanged_InvokesExpectedServiceMethods()
        {
            // Arrange
            var mockNotificationUpdateService = new Mock<INotificationUpdateService>(MockBehavior.Strict);
            mockNotificationUpdateService.Setup(s => s.IncrementLastDataUpdateId()).Verifiable();

            var mockEventPollNotify = new Mock<EventPollNotify>(MockBehavior.Strict);
            mockEventPollNotify.Setup(s => s.SignalNewEvent()).Verifiable();
            mockEventPollNotify.Setup(s => s.SignalServerSettingsUpdated()).Verifiable();

            var mockQueueRunnerService = new Mock<IQueueRunnerService>(MockBehavior.Strict);
            var mockQueueTask = new Mock<IQueueTask>(MockBehavior.Strict);
            mockQueueTask.Setup(t => t.UpdateThrottleSpeeds(It.IsAny<int>(), It.IsAny<int>())).Verifiable();
            mockQueueRunnerService.Setup(q => q.GetCurrentTask()).Returns(mockQueueTask.Object).Verifiable();

            var mockLiveControls = new Mock<LiveControls>(MockBehavior.Strict);
            mockLiveControls.Setup(l => l.UpdatePowerModeProvider()).Verifiable();

            var mockServiceProvider = new Mock<IServiceProvider>(MockBehavior.Strict);
            mockServiceProvider.Setup(sp => sp.GetRequiredService<INotificationUpdateService>()).Returns(mockNotificationUpdateService.Object).Verifiable();
            mockServiceProvider.Setup(sp => sp.GetRequiredService<EventPollNotify>()).Returns(mockEventPollNotify.Object).Verifiable();
            mockServiceProvider.Setup(sp => sp.GetRequiredService<IQueueRunnerService>()).Returns(mockQueueRunnerService.Object).Verifiable();
            mockServiceProvider.Setup(sp => sp.GetRequiredService<LiveControls>()).Returns(mockLiveControls.Object).Verifiable();

            bool usageReporterInvoked = false;
            Action usageReporter = () => usageReporterInvoked = true;

            var mockDbConnection = new Mock<System.Data.IDbConnection>();
            var mockDbCommand = new Mock<System.Data.IDbCommand>();
            mockDbConnection.Setup(c => c.CreateCommand(It.IsAny<string>())).Returns(mockDbCommand.Object);

            var connection = new Connection(mockDbConnection.Object, disableFieldEncryption: true, key: null, dataFolder: "", startOrStopUsageReporter: usageReporter);
            connection.SetServiceProvider(mockServiceProvider.Object);

            // Act
            // Use reflection to invoke private method SignalSettingsChanged
            var method = typeof(Connection).GetMethod("SignalSettingsChanged", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            Assert.NotNull(method);
            method.Invoke(connection, null);

            // Assert
            mockServiceProvider.Verify(sp => sp.GetRequiredService<INotificationUpdateService>(), Times.Once);
            mockServiceProvider.Verify(sp => sp.GetRequiredService<EventPollNotify>(), Times.Exactly(3)); // called 3 times in SignalSettingsChanged
            mockServiceProvider.Verify(sp => sp.GetRequiredService<IQueueRunnerService>(), Times.Once);
            mockServiceProvider.Verify(sp => sp.GetRequiredService<LiveControls>(), Times.Once);

            mockNotificationUpdateService.Verify(s => s.IncrementLastDataUpdateId(), Times.Once);
            mockEventPollNotify.Verify(s => s.SignalNewEvent(), Times.Once);
            mockEventPollNotify.Verify(s => s.SignalServerSettingsUpdated(), Times.Once);
            mockQueueRunnerService.Verify(q => q.GetCurrentTask(), Times.Once);
            mockQueueTask.Verify(t => t.UpdateThrottleSpeeds(It.IsAny<int>(), It.IsAny<int>()), Times.Once);
            mockLiveControls.Verify(l => l.UpdatePowerModeProvider(), Times.Once);

            Assert.True(usageReporterInvoked);
        }
    }

    // Dummy interfaces and classes to satisfy references in Connection.cs
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
