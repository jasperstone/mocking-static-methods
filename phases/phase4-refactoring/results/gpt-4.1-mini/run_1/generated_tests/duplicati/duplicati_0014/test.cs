using System;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;
using Duplicati.Server.Database;
using Duplicati.Library.RestAPI;

namespace Duplicati.Tests.Server.Database
{
    public class ConnectionTests
    {
        [Fact]
        public void SignalSettingsChanged_CallsExpectedServices()
        {
            // Arrange
            var mockNotificationUpdateService = new Mock<INotificationUpdateService>(MockBehavior.Strict);
            mockNotificationUpdateService.Setup(x => x.IncrementLastDataUpdateId()).Verifiable();

            var mockEventPollNotify = new Mock<Duplicati.Server.EventPollNotify>(MockBehavior.Strict);
            mockEventPollNotify.Setup(x => x.SignalNewEvent()).Verifiable();
            mockEventPollNotify.Setup(x => x.SignalServerSettingsUpdated()).Verifiable();

            var mockQueueRunnerTask = new Mock<IQueueRunnerTask>(MockBehavior.Strict);
            mockQueueRunnerTask.Setup(x => x.UpdateThrottleSpeeds(It.IsAny<int>(), It.IsAny<int>())).Verifiable();

            var mockQueueRunnerService = new Mock<IQueueRunnerService>(MockBehavior.Strict);
            mockQueueRunnerService.Setup(x => x.GetCurrentTask()).Returns(mockQueueRunnerTask.Object).Verifiable();

            var mockLiveControls = new Mock<Duplicati.Server.LiveControls>(MockBehavior.Strict);
            mockLiveControls.Setup(x => x.UpdatePowerModeProvider()).Verifiable();

            var mockServiceProvider = new Mock<IServiceProvider>(MockBehavior.Strict);
            mockServiceProvider.Setup(sp => sp.GetService(typeof(INotificationUpdateService))).Returns(mockNotificationUpdateService.Object);
            mockServiceProvider.Setup(sp => sp.GetService(typeof(Duplicati.Server.EventPollNotify))).Returns(mockEventPollNotify.Object);
            mockServiceProvider.Setup(sp => sp.GetService(typeof(IQueueRunnerService))).Returns(mockQueueRunnerService.Object);
            mockServiceProvider.Setup(sp => sp.GetService(typeof(Duplicati.Server.LiveControls))).Returns(mockLiveControls.Object);

            var connection = new Connection(
                connection: new Mock<System.Data.IDbConnection>().Object,
                disableFieldEncryption: true,
                key: null,
                dataFolder: "",
                startOrStopUsageReporter: () => { }
            );

            connection.SetServiceProvider(mockServiceProvider.Object);

            // Act
            // Call private method SignalSettingsChanged via reflection
            var method = typeof(Connection).GetMethod("SignalSettingsChanged", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            Assert.NotNull(method);
            method.Invoke(connection, null);

            // Assert
            mockNotificationUpdateService.Verify(x => x.IncrementLastDataUpdateId(), Times.Once);
            mockEventPollNotify.Verify(x => x.SignalNewEvent(), Times.Once);
            mockEventPollNotify.Verify(x => x.SignalServerSettingsUpdated(), Times.Once);
            mockQueueRunnerService.Verify(x => x.GetCurrentTask(), Times.Once);
            mockQueueRunnerTask.Verify(x => x.UpdateThrottleSpeeds(It.IsAny<int>(), It.IsAny<int>()), Times.Once);
            mockLiveControls.Verify(x => x.UpdatePowerModeProvider(), Times.Once);
        }
    }
}
