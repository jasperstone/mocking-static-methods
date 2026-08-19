using System;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;
using Duplicati.Server.Database;
using Duplicati.Library.RestAPI;
using Duplicati.Server;

namespace Duplicati.Tests.Server.Database
{
    public class ConnectionTests
    {
        [Fact]
        public void SignalSettingsChanged_InvokesExpectedServiceMethods()
        {
            // Arrange
            var mockNotificationUpdateService = new Mock<INotificationUpdateService>(MockBehavior.Strict);
            mockNotificationUpdateService.Setup(x => x.IncrementLastDataUpdateId()).Verifiable();

            var mockEventPollNotify = new Mock<EventPollNotify>(MockBehavior.Strict);
            mockEventPollNotify.Setup(x => x.SignalNewEvent()).Verifiable();
            mockEventPollNotify.Setup(x => x.SignalServerSettingsUpdated()).Verifiable();

            var mockQueueTask = new Mock<IQueueTask>(MockBehavior.Strict);
            mockQueueTask.Setup(x => x.UpdateThrottleSpeeds(It.IsAny<string>(), It.IsAny<string>())).Verifiable();

            var mockQueueRunnerService = new Mock<IQueueRunnerService>(MockBehavior.Strict);
            mockQueueRunnerService.Setup(x => x.GetCurrentTask()).Returns(mockQueueTask.Object).Verifiable();

            var mockLiveControls = new Mock<LiveControls>(MockBehavior.Strict, new object[] { null! });
            mockLiveControls.Setup(x => x.UpdatePowerModeProvider()).Verifiable();

            var mockServiceProvider = new Mock<IServiceProvider>(MockBehavior.Strict);
            mockServiceProvider.Setup(sp => sp.GetService(typeof(INotificationUpdateService))).Returns(mockNotificationUpdateService.Object);
            mockServiceProvider.Setup(sp => sp.GetService(typeof(EventPollNotify))).Returns(mockEventPollNotify.Object);
            mockServiceProvider.Setup(sp => sp.GetService(typeof(IQueueRunnerService))).Returns(mockQueueRunnerService.Object);
            mockServiceProvider.Setup(sp => sp.GetService(typeof(LiveControls))).Returns(mockLiveControls.Object);

            var mockDbConnection = new Mock<System.Data.IDbConnection>();
            var mockDbCommand = new Mock<System.Data.IDbCommand>();
            mockDbConnection.Setup(c => c.CreateCommand()).Returns(mockDbCommand.Object);

            var connection = new Connection(mockDbConnection.Object, disableFieldEncryption: false, key: null, dataFolder: "", startOrStopUsageReporter: () => { });
            connection.SetServiceProvider(mockServiceProvider.Object);

            // Act
            var method = typeof(Connection).GetMethod("SignalSettingsChanged", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            Assert.NotNull(method);
            method.Invoke(connection, null);

            // Assert
            mockNotificationUpdateService.Verify(x => x.IncrementLastDataUpdateId(), Times.Once);
            mockEventPollNotify.Verify(x => x.SignalNewEvent(), Times.Once);
            mockEventPollNotify.Verify(x => x.SignalServerSettingsUpdated(), Times.Once);
            mockQueueRunnerService.Verify(x => x.GetCurrentTask(), Times.Once);
            mockQueueTask.Verify(x => x.UpdateThrottleSpeeds(It.IsAny<string>(), It.IsAny<string>()), Times.Once);
            mockLiveControls.Verify(x => x.UpdatePowerModeProvider(), Times.Once);
        }
    }
}
