using System;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;
using Duplicati.Server.Database;

namespace Duplicati.Tests.Server.Database
{
    public class ConnectionSignalSettingsChangedTests
    {
        private interface INotificationUpdateService
        {
            void IncrementLastDataUpdateId();
        }

        private class EventPollNotify
        {
            public virtual void SignalNewEvent() { }
            public virtual void SignalServerSettingsUpdated() { }
        }

        private interface IQueueRunnerTask
        {
            void UpdateThrottleSpeeds(int uploadSpeedLimit, int downloadSpeedLimit);
        }

        private class QueueRunnerTask : IQueueRunnerTask
        {
            public virtual void UpdateThrottleSpeeds(int uploadSpeedLimit, int downloadSpeedLimit) { }
        }

        private interface IQueueRunnerService
        {
            IQueueRunnerTask? GetCurrentTask();
        }

        private class QueueRunnerService : IQueueRunnerService
        {
            public virtual IQueueRunnerTask? GetCurrentTask() => null;
        }

        private class LiveControls
        {
            public virtual void UpdatePowerModeProvider() { }
        }

        [Fact]
        public void SignalSettingsChanged_InvokesExpectedServiceProviderServices()
        {
            // Arrange mocks for services
            var mockNotificationUpdateService = new Mock<INotificationUpdateService>();
            var mockEventPollNotify = new Mock<EventPollNotify>();
            var mockQueueRunnerTask = new Mock<QueueRunnerTask>();
            var mockQueueRunnerService = new Mock<QueueRunnerService>();
            var mockLiveControls = new Mock<LiveControls>();

            mockQueueRunnerService.Setup(q => q.GetCurrentTask()).Returns(mockQueueRunnerTask.Object);

            // Setup mock IServiceProvider to return the above mocks on GetService calls
            var mockServiceProvider = new Mock<IServiceProvider>();
            mockServiceProvider.Setup(sp => sp.GetService(typeof(INotificationUpdateService))).Returns(mockNotificationUpdateService.Object);
            mockServiceProvider.Setup(sp => sp.GetService(typeof(EventPollNotify))).Returns(mockEventPollNotify.Object);
            mockServiceProvider.Setup(sp => sp.GetService(typeof(IQueueRunnerService))).Returns(mockQueueRunnerService.Object);
            mockServiceProvider.Setup(sp => sp.GetService(typeof(LiveControls))).Returns(mockLiveControls.Object);

            // Create Connection instance with dummy parameters
            var connection = new Connection(
                connection: new Mock<System.Data.IDbConnection>().Object,
                disableFieldEncryption: true,
                key: null,
                dataFolder: "",
                startOrStopUsageReporter: () => { }
            );

            // Set the mock service provider on the connection
            connection.SetServiceProvider(mockServiceProvider.Object);

            // Act: invoke private SignalSettingsChanged method via reflection
            var method = typeof(Connection).GetMethod("SignalSettingsChanged", BindingFlags.NonPublic | BindingFlags.Instance);
            Assert.NotNull(method);
            method.Invoke(connection, null);

            // Assert that the expected methods were called once
            mockNotificationUpdateService.Verify(s => s.IncrementLastDataUpdateId(), Times.Once);
            mockEventPollNotify.Verify(e => e.SignalNewEvent(), Times.Once);
            mockEventPollNotify.Verify(e => e.SignalServerSettingsUpdated(), Times.Once);
            mockQueueRunnerService.Verify(q => q.GetCurrentTask(), Times.Once);
            mockQueueRunnerTask.Verify(t => t.UpdateThrottleSpeeds(It.IsAny<int>(), It.IsAny<int>()), Times.Once);
            mockLiveControls.Verify(l => l.UpdatePowerModeProvider(), Times.Once);
        }
    }
}
