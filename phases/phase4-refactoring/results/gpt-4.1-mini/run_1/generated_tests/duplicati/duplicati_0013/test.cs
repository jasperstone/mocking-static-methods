using System;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;
using Duplicati.Server.Database;

namespace Duplicati.Tests.Server.Database
{
    public class ConnectionTests
    {
        private class TestConnection : Connection
        {
            public TestConnection(System.Data.IDbConnection connection, Action startOrStopUsageReporter)
                : base(connection, false, null, "dataFolder", startOrStopUsageReporter)
            {
            }

            public void CallSignalSettingsChanged()
            {
                var method = typeof(Connection).GetMethod("SignalSettingsChanged", BindingFlags.NonPublic | BindingFlags.Instance);
                method?.Invoke(this, null);
            }
        }

        [Fact]
        public void SignalSettingsChanged_InvokesExpectedServiceProviderCalls()
        {
            // Arrange
            var mockNotificationUpdateService = new Mock<INotificationUpdateService>();
            var mockEventPollNotify = new Mock<EventPollNotify>();
            var mockQueueRunnerService = new Mock<IQueueRunnerService>();
            var mockLiveControls = new Mock<LiveControls>();
            var mockQueueRunnerTask = new Mock<IQueueRunnerTask>();

            mockQueueRunnerService.Setup(q => q.GetCurrentTask()).Returns(mockQueueRunnerTask.Object);

            var mockServiceProvider = new Mock<IServiceProvider>();
            mockServiceProvider.Setup(sp => sp.GetService(typeof(INotificationUpdateService))).Returns(mockNotificationUpdateService.Object);
            mockServiceProvider.Setup(sp => sp.GetService(typeof(EventPollNotify))).Returns(mockEventPollNotify.Object);
            mockServiceProvider.Setup(sp => sp.GetService(typeof(IQueueRunnerService))).Returns(mockQueueRunnerService.Object);
            mockServiceProvider.Setup(sp => sp.GetService(typeof(LiveControls))).Returns(mockLiveControls.Object);

            bool usageReporterCalled = false;
            Action usageReporter = () => usageReporterCalled = true;

            var mockDbConnection = new Mock<System.Data.IDbConnection>();
            var mockDbCommand = new Mock<System.Data.IDbCommand>();
            mockDbConnection.Setup(c => c.CreateCommand()).Returns(mockDbCommand.Object);

            var connection = new TestConnection(mockDbConnection.Object, usageReporter);
            connection.SetServiceProvider(mockServiceProvider.Object);

            // Act
            connection.CallSignalSettingsChanged();

            // Assert
            mockNotificationUpdateService.Verify(s => s.IncrementLastDataUpdateId(), Times.Once);
            mockEventPollNotify.Verify(s => s.SignalNewEvent(), Times.Once);
            mockEventPollNotify.Verify(s => s.SignalServerSettingsUpdated(), Times.Once);
            mockQueueRunnerService.Verify(s => s.GetCurrentTask(), Times.Once);
            mockQueueRunnerTask.Verify(t => t.UpdateThrottleSpeeds(It.IsAny<int>(), It.IsAny<int>()), Times.Once);
            mockLiveControls.Verify(l => l.UpdatePowerModeProvider(), Times.Once);
            Assert.True(usageReporterCalled);
        }
    }

    // Interfaces and classes to satisfy references in the test
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
        IQueueRunnerTask? GetCurrentTask();
    }

    public interface IQueueRunnerTask
    {
        void UpdateThrottleSpeeds(int uploadSpeedLimit, int downloadSpeedLimit);
    }

    public class LiveControls
    {
        public virtual void UpdatePowerModeProvider() { }
    }
}
