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
        public void SignalSettingsChanged_CallsGetRequiredServiceAndMethods()
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
            mockServiceProvider.Setup(sp => sp.GetRequiredService<INotificationUpdateService>()).Returns(mockNotificationUpdateService.Object);
            mockServiceProvider.Setup(sp => sp.GetRequiredService<EventPollNotify>()).Returns(mockEventPollNotify.Object);
            mockServiceProvider.Setup(sp => sp.GetRequiredService<IQueueRunnerService>()).Returns(mockQueueRunnerService.Object);
            mockServiceProvider.Setup(sp => sp.GetRequiredService<LiveControls>()).Returns(mockLiveControls.Object);

            var connection = new Connection(
                connection: new Mock<System.Data.IDbConnection>().Object,
                disableFieldEncryption: true,
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
            mockServiceProvider.Verify(sp => sp.GetRequiredService<INotificationUpdateService>(), Times.AtLeastOnce);
            mockServiceProvider.Verify(sp => sp.GetRequiredService<EventPollNotify>(), Times.Exactly(3)); // called 3 times
            mockServiceProvider.Verify(sp => sp.GetRequiredService<IQueueRunnerService>(), Times.Once);
            mockServiceProvider.Verify(sp => sp.GetRequiredService<LiveControls>(), Times.Once);

            mockNotificationUpdateService.Verify(x => x.IncrementLastDataUpdateId(), Times.Once);
            mockEventPollNotify.Verify(x => x.SignalNewEvent(), Times.Once);
            mockEventPollNotify.Verify(x => x.SignalServerSettingsUpdated(), Times.Once);
            mockQueueTask.Verify(x => x.UpdateThrottleSpeeds(It.IsAny<int>(), It.IsAny<int>()), Times.Once);
            mockLiveControls.Verify(x => x.UpdatePowerModeProvider(), Times.Once);
        }
    }
}
