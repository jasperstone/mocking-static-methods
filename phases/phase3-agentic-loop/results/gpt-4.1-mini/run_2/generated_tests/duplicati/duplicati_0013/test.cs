using System;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;
using Duplicati.Server.Database;
using System.Data;

namespace Duplicati.Tests.Server.Database
{
    public class ConnectionTests
    {
        [Fact]
        public void SignalSettingsChanged_CallsExpectedServices()
        {
            // Arrange
            var mockNotificationService = new Mock<INotificationUpdateService>();
            var mockEventPollNotify = new Mock<EventPollNotify>();
            var mockQueueRunnerService = new Mock<IQueueRunnerService>();
            var mockLiveControls = new Mock<LiveControls>();
            var mockQueueTask = new Mock<IQueueTask>();

            mockQueueRunnerService.Setup(q => q.GetCurrentTask()).Returns(mockQueueTask.Object);

            var mockServiceProvider = new Mock<IServiceProvider>();
            mockServiceProvider.Setup(sp => sp.GetService(typeof(INotificationUpdateService))).Returns(mockNotificationService.Object);
            mockServiceProvider.Setup(sp => sp.GetService(typeof(EventPollNotify))).Returns(mockEventPollNotify.Object);
            mockServiceProvider.Setup(sp => sp.GetService(typeof(IQueueRunnerService))).Returns(mockQueueRunnerService.Object);
            mockServiceProvider.Setup(sp => sp.GetService(typeof(LiveControls))).Returns(mockLiveControls.Object);

            // Mock IDbConnection and IDbCommand to satisfy Connection constructor
            var mockDbCommand = new Mock<IDbCommand>();
            mockDbCommand.SetupAllProperties();
            mockDbCommand.Setup(c => c.ExecuteNonQuery()).Returns(0);

            var mockDbConnection = new Mock<IDbConnection>();
            mockDbConnection.Setup(c => c.CreateCommand()).Returns(mockDbCommand.Object);

            var connection = new Connection(
                connection: mockDbConnection.Object,
                disableFieldEncryption: true,
                key: null,
                dataFolder: "",
                startOrStopUsageReporter: () => { }
            );

            connection.SetServiceProvider(mockServiceProvider.Object);

            // Use reflection to invoke private method SignalSettingsChanged
            var method = typeof(Connection).GetMethod("SignalSettingsChanged", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            Assert.NotNull(method);

            // Act
            method.Invoke(connection, null);

            // Assert
            mockNotificationService.Verify(n => n.IncrementLastDataUpdateId(), Times.Once);
            mockEventPollNotify.Verify(e => e.SignalNewEvent(), Times.Once);
            mockEventPollNotify.Verify(e => e.SignalServerSettingsUpdated(), Times.Once);
            mockQueueRunnerService.Verify(q => q.GetCurrentTask(), Times.Once);
            mockQueueTask.Verify(t => t.UpdateThrottleSpeeds(It.IsAny<int>(), It.IsAny<int>()), Times.Once);
            mockLiveControls.Verify(l => l.UpdatePowerModeProvider(), Times.Once);
        }
    }

    // Dummy interfaces/classes to satisfy references in the tested code
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

    // Stub for ApplicationSettings static properties used in tested method
    public static class ApplicationSettings
    {
        public static int UploadSpeedLimit { get; set; } = 100;
        public static int DownloadSpeedLimit { get; set; } = 200;
    }
}
