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
        public void SignalSettingsChanged_CallsExpectedServiceMethods()
        {
            // Arrange
            var mockNotificationUpdateService = new Mock<INotificationUpdateService>();
            mockNotificationUpdateService.Setup(x => x.IncrementLastDataUpdateId());

            var mockEventPollNotify = new Mock<EventPollNotify>();
            mockEventPollNotify.Setup(x => x.SignalNewEvent());
            mockEventPollNotify.Setup(x => x.SignalServerSettingsUpdated());

            var mockQueueRunnerService = new Mock<IQueueRunnerService>();
            var mockQueueTask = new Mock<IQueueTask>();
            mockQueueTask.Setup(x => x.UpdateThrottleSpeeds(It.IsAny<int>(), It.IsAny<int>()));
            mockQueueRunnerService.Setup(x => x.GetCurrentTask()).Returns(mockQueueTask.Object);

            var mockLiveControls = new Mock<LiveControls>();
            mockLiveControls.Setup(x => x.UpdatePowerModeProvider());

            var mockServiceProvider = new Mock<IServiceProvider>();
            // Setup GetService instead of GetRequiredService to avoid Moq limitation with extension methods
            mockServiceProvider.Setup(sp => sp.GetService(typeof(INotificationUpdateService))).Returns(mockNotificationUpdateService.Object);
            mockServiceProvider.Setup(sp => sp.GetService(typeof(EventPollNotify))).Returns(mockEventPollNotify.Object);
            mockServiceProvider.Setup(sp => sp.GetService(typeof(IQueueRunnerService))).Returns(mockQueueRunnerService.Object);
            mockServiceProvider.Setup(sp => sp.GetService(typeof(LiveControls))).Returns(mockLiveControls.Object);

            var mockDbCommand = new Mock<IDbCommand>();
            mockDbCommand.SetupAllProperties();
            mockDbCommand.Setup(cmd => cmd.CommandText).Returns(string.Empty);
            mockDbCommand.Setup(cmd => cmd.Parameters).Returns(new Mock<IDataParameterCollection>().Object);
            mockDbCommand.Setup(cmd => cmd.Dispose());

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

            // Act
            // We invoke the private method SignalSettingsChanged via reflection because it is private
            var method = typeof(Connection).GetMethod("SignalSettingsChanged", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            Assert.NotNull(method);
            method.Invoke(connection, null);

            // Assert
            mockNotificationUpdateService.Verify(x => x.IncrementLastDataUpdateId(), Times.Once);
            mockEventPollNotify.Verify(x => x.SignalNewEvent(), Times.Once);
            mockEventPollNotify.Verify(x => x.SignalServerSettingsUpdated(), Times.Once);
            mockQueueTask.Verify(x => x.UpdateThrottleSpeeds(It.IsAny<int>(), It.IsAny<int>()), Times.Once);
            mockLiveControls.Verify(x => x.UpdatePowerModeProvider(), Times.Once);
        }
    }

    // Dummy interfaces and classes to satisfy references in the tested code
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
