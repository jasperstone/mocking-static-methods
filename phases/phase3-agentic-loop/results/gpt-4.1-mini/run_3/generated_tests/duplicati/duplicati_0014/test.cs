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
            var mockNotificationUpdateService = new Mock<INotificationUpdateService>(MockBehavior.Strict);
            mockNotificationUpdateService.Setup(x => x.IncrementLastDataUpdateId());

            var mockEventPollNotify = new Mock<EventPollNotify>(MockBehavior.Strict);
            mockEventPollNotify.Setup(x => x.SignalNewEvent());
            mockEventPollNotify.Setup(x => x.SignalServerSettingsUpdated());

            var mockQueueRunnerService = new Mock<IQueueRunnerService>(MockBehavior.Strict);
            var mockQueueTask = new Mock<IQueueTask>(MockBehavior.Strict);
            mockQueueTask.Setup(x => x.UpdateThrottleSpeeds(It.IsAny<int>(), It.IsAny<int>()));
            mockQueueRunnerService.Setup(x => x.GetCurrentTask()).Returns(mockQueueTask.Object);

            var mockLiveControls = new Mock<LiveControls>(MockBehavior.Strict);
            mockLiveControls.Setup(x => x.UpdatePowerModeProvider());

            // We cannot mock extension method GetRequiredService directly, so we mock IServiceProvider.GetService instead
            var mockServiceProvider = new Mock<IServiceProvider>(MockBehavior.Strict);
            mockServiceProvider.Setup(sp => sp.GetService(typeof(INotificationUpdateService))).Returns(mockNotificationUpdateService.Object);
            mockServiceProvider.Setup(sp => sp.GetService(typeof(EventPollNotify))).Returns(mockEventPollNotify.Object);
            mockServiceProvider.Setup(sp => sp.GetService(typeof(IQueueRunnerService))).Returns(mockQueueRunnerService.Object);
            mockServiceProvider.Setup(sp => sp.GetService(typeof(LiveControls))).Returns(mockLiveControls.Object);

            bool usageReporterInvoked = false;
            Action usageReporter = () => usageReporterInvoked = true;

            // Setup a mock IDbCommand that returns expected behavior for SetCommandAndParameters call in ExtensionMethods
            var mockDbCommand = new Mock<IDbCommand>(MockBehavior.Strict);
            mockDbCommand.SetupAllProperties();
            mockDbCommand.Setup(cmd => cmd.Dispose());
            mockDbCommand.Setup(cmd => cmd.ExecuteNonQuery()).Returns(0);
            // Setup for SetCommandAndParameters extension method: it returns the same command instance
            // We cannot mock extension methods, so we rely on the method returning the same instance

            var mockDbConnection = new Mock<IDbConnection>(MockBehavior.Strict);
            mockDbConnection.Setup(conn => conn.CreateCommand()).Returns(mockDbCommand.Object);

            var connection = new Connection(mockDbConnection.Object, false, null, "dataFolder", usageReporter);

            // We set the private field m_serviceProvider directly because SetServiceProvider calls GetRequiredService which we cannot mock
            var serviceProviderField = typeof(Connection).GetField("m_serviceProvider", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            Assert.NotNull(serviceProviderField);
            serviceProviderField.SetValue(connection, mockServiceProvider.Object);

            // Act
            var method = typeof(Connection).GetMethod("SignalSettingsChanged", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            Assert.NotNull(method);
            method.Invoke(connection, null);

            // Assert
            mockNotificationUpdateService.Verify(x => x.IncrementLastDataUpdateId(), Times.Once);
            mockEventPollNotify.Verify(x => x.SignalNewEvent(), Times.Once);
            mockEventPollNotify.Verify(x => x.SignalServerSettingsUpdated(), Times.Once);
            mockQueueRunnerService.Verify(x => x.GetCurrentTask(), Times.Once);
            mockQueueTask.Verify(x => x.UpdateThrottleSpeeds(It.IsAny<int>(), It.IsAny<int>()), Times.Once);
            mockLiveControls.Verify(x => x.UpdatePowerModeProvider(), Times.Once);
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
