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
        public void SignalSettingsChanged_CallsGetRequiredServiceAndMethods()
        {
            // Arrange
            var mockNotificationService = new Mock<INotificationUpdateService>();
            mockNotificationService.Setup(x => x.IncrementLastDataUpdateId());

            var mockEventPollNotify = new Mock<EventPollNotify>();
            mockEventPollNotify.Setup(x => x.SignalNewEvent());
            mockEventPollNotify.Setup(x => x.SignalServerSettingsUpdated());

            var mockQueueRunnerTask = new Mock<IQueueRunnerTask>();
            mockQueueRunnerTask.Setup(t => t.UpdateThrottleSpeeds(It.IsAny<string>(), It.IsAny<string>()));

            var mockQueueRunnerService = new Mock<IQueueRunnerService>();
            mockQueueRunnerService.Setup(q => q.GetCurrentTask()).Returns(mockQueueRunnerTask.Object);

            var mockLiveControls = new Mock<LiveControls>();
            mockLiveControls.Setup(l => l.UpdatePowerModeProvider());

            var mockServiceProvider = new Mock<IServiceProvider>();
            mockServiceProvider.Setup(sp => sp.GetRequiredService<INotificationUpdateService>()).Returns(mockNotificationService.Object);
            mockServiceProvider.Setup(sp => sp.GetRequiredService<EventPollNotify>()).Returns(mockEventPollNotify.Object);
            mockServiceProvider.Setup(sp => sp.GetRequiredService<IQueueRunnerService>()).Returns(mockQueueRunnerService.Object);
            mockServiceProvider.Setup(sp => sp.GetRequiredService<LiveControls>()).Returns(mockLiveControls.Object);

            var connection = new Connection(
                connection: new Mock<System.Data.IDbConnection>().Object,
                disableFieldEncryption: false,
                key: null,
                dataFolder: "",
                startOrStopUsageReporter: () => { }
            );

            connection.SetServiceProvider(mockServiceProvider.Object);

            // Act
            // Use reflection to invoke private method SignalSettingsChanged
            var method = typeof(Connection).GetMethod("SignalSettingsChanged", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            Assert.NotNull(method);
            method.Invoke(connection, null);

            // Assert
            mockNotificationService.Verify(x => x.IncrementLastDataUpdateId(), Times.Once);
            mockEventPollNotify.Verify(x => x.SignalNewEvent(), Times.Once);
            mockEventPollNotify.Verify(x => x.SignalServerSettingsUpdated(), Times.Once);
            mockQueueRunnerService.Verify(x => x.GetCurrentTask(), Times.Once);
            mockQueueRunnerTask.Verify(t => t.UpdateThrottleSpeeds(It.IsAny<string>(), It.IsAny<string>()), Times.Once);
            mockLiveControls.Verify(l => l.UpdatePowerModeProvider(), Times.Once);
        }
    }
}
