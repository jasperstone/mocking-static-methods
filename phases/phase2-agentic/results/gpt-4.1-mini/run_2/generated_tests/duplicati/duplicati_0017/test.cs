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
            var mockServiceProvider = new Mock<IServiceProvider>(MockBehavior.Strict);

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

            // Setup IServiceProvider.GetRequiredService<T> calls
            mockServiceProvider.Setup(sp => sp.GetRequiredService<INotificationUpdateService>())
                .Returns(mockNotificationUpdateService.Object).Verifiable();
            mockServiceProvider.Setup(sp => sp.GetRequiredService<EventPollNotify>())
                .Returns(mockEventPollNotify.Object).Verifiable();
            mockServiceProvider.Setup(sp => sp.GetRequiredService<IQueueRunnerService>())
                .Returns(mockQueueRunnerService.Object).Verifiable();
            mockServiceProvider.Setup(sp => sp.GetRequiredService<LiveControls>())
                .Returns(mockLiveControls.Object).Verifiable();

            // Setup Connection with a dummy IDbConnection and other required parameters
            var mockDbConnection = new Mock<System.Data.IDbConnection>();
            mockDbConnection.Setup(c => c.CreateCommand()).Returns(Mock.Of<System.Data.IDbCommand>());

            bool usageReporterCalled = false;
            Action usageReporter = () => usageReporterCalled = true;

            var connection = new Connection(mockDbConnection.Object, disableFieldEncryption: true, key: null, dataFolder: "", startOrStopUsageReporter: usageReporter);
            connection.SetServiceProvider(mockServiceProvider.Object);

            // Act
            // Use reflection to invoke private method SignalSettingsChanged
            var method = typeof(Connection).GetMethod("SignalSettingsChanged", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            Assert.NotNull(method);
            method.Invoke(connection, null);

            // Assert
            mockServiceProvider.Verify(sp => sp.GetRequiredService<INotificationUpdateService>(), Times.Once);
            mockServiceProvider.Verify(sp => sp.GetRequiredService<EventPollNotify>(), Times.Exactly(3)); // Called 3 times in code
            mockServiceProvider.Verify(sp => sp.GetRequiredService<IQueueRunnerService>(), Times.Once);
            mockServiceProvider.Verify(sp => sp.GetRequiredService<LiveControls>(), Times.Once);

            mockNotificationUpdateService.Verify(s => s.IncrementLastDataUpdateId(), Times.Once);
            mockEventPollNotify.Verify(s => s.SignalNewEvent(), Times.Once);
            mockEventPollNotify.Verify(s => s.SignalServerSettingsUpdated(), Times.Once);
            mockQueueRunnerService.Verify(q => q.GetCurrentTask(), Times.Once);
            mockQueueTask.Verify(t => t.UpdateThrottleSpeeds(It.IsAny<int>(), It.IsAny<int>()), Times.Once);
            mockLiveControls.Verify(l => l.UpdatePowerModeProvider(), Times.Once);

            Assert.True(usageReporterCalled);
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
