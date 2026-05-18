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
            // Setup the service provider to return the mocks directly (not using GetRequiredService extension)
            mockServiceProvider.Setup(sp => sp.GetService(typeof(INotificationUpdateService)))
                .Returns(mockNotificationUpdateService.Object);
            mockServiceProvider.Setup(sp => sp.GetService(typeof(EventPollNotify)))
                .Returns(mockEventPollNotify.Object);
            mockServiceProvider.Setup(sp => sp.GetService(typeof(IQueueRunnerService)))
                .Returns(mockQueueRunnerService.Object);
            mockServiceProvider.Setup(sp => sp.GetService(typeof(LiveControls)))
                .Returns(mockLiveControls.Object);

            // Setup IDbCommand to support SetCommandAndParameters call (mocking extension method dependency)
            var mockCommand = new Mock<System.Data.IDbCommand>();
            // Setup for SetCommandAndParameters extension method: it likely sets CommandText and returns the command itself
            mockCommand.SetupProperty(c => c.CommandText);
            mockCommand.Setup(c => c.Parameters).Returns(new System.Data.SqlClient.SqlParameterCollection());

            // Setup IDbConnection to return the mockCommand
            var mockConnection = new Mock<System.Data.IDbConnection>();
            mockConnection.Setup(c => c.CreateCommand()).Returns(mockCommand.Object);

            bool usageReporterCalled = false;
            Action usageReporter = () => usageReporterCalled = true;

            var connection = new Connection(mockConnection.Object, false, null, "dataFolder", usageReporter);
            connection.SetServiceProvider(mockServiceProvider.Object);

            // Act
            // Call private method SignalSettingsChanged via reflection
            var method = typeof(Connection).GetMethod("SignalSettingsChanged", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            Assert.NotNull(method);
            method.Invoke(connection, null);

            // Assert
            mockNotificationUpdateService.Verify(n => n.IncrementLastDataUpdateId(), Times.Never); // Not called in SignalSettingsChanged
            mockEventPollNotify.Verify(e => e.SignalNewEvent(), Times.Once);
            mockEventPollNotify.Verify(e => e.SignalServerSettingsUpdated(), Times.Once);
            mockQueueRunnerService.Verify(q => q.GetCurrentTask(), Times.Once);
            mockQueueTask.Verify(t => t.UpdateThrottleSpeeds(It.IsAny<int>(), It.IsAny<int>()), Times.Once);
            mockLiveControls.Verify(l => l.UpdatePowerModeProvider(), Times.Once);
            Assert.True(usageReporterCalled);
        }
    }

    // Dummy interfaces and classes to satisfy references
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
