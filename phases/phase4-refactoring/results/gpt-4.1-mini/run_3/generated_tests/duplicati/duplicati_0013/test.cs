using System;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;
using Duplicati.Server.Database;
using Duplicati.WebserverCore.Abstractions;

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

            var connection = new Connection(
                connection: new Mock<System.Data.IDbConnection>().Object,
                disableFieldEncryption: true,
                key: null,
                dataFolder: "",
                startOrStopUsageReporter: () => { }
            );

            connection.SetServiceProvider(mockServiceProvider.Object);

            // Act
            // We invoke the private method SignalSettingsChanged via reflection since it's private
            var method = typeof(Connection).GetMethod("SignalSettingsChanged", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            Assert.NotNull(method);
            method.Invoke(connection, null);

            // Assert
            mockNotificationService.Verify(n => n.IncrementLastDataUpdateId(), Times.Once);
            mockEventPollNotify.Verify(e => e.SignalNewEvent(), Times.Once);
            mockEventPollNotify.Verify(e => e.SignalServerSettingsUpdated(), Times.Once);
            mockQueueTask.Verify(q => q.UpdateThrottleSpeeds(It.IsAny<int>(), It.IsAny<int>()), Times.Once);
            mockLiveControls.Verify(l => l.UpdatePowerModeProvider(), Times.Once);
        }
    }
}
