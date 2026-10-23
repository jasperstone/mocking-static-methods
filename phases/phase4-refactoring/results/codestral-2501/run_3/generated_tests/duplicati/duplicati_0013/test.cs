using Xunit;
using Moq;
using System;
using Duplicati.Server.Database;
using Microsoft.Extensions.DependencyInjection;
using Duplicati.Library.Interface;
using Duplicati.Library.Main;
using Duplicati.Library.RestAPI;
using Duplicati.Library.Encryption;
using Duplicati.Library.DynamicLoader;
using Duplicati.Library.AutoUpdater;
using System.Data;
using Duplicati.Library.Main.Database;
using System.Globalization;
using Duplicati.WebserverCore.Abstractions;
using System.Text.Json;

namespace Duplicati.Tests
{
    public class ConnectionTests
    {
        [Fact]
        public void SignalSettingsChanged_CallsDependencies()
        {
            // Arrange
            var mockServiceProvider = new Mock<IServiceProvider>();
            var mockNotificationUpdateService = new Mock<INotificationUpdateService>();
            var mockEventPollNotify = new Mock<EventPollNotify>();
            var mockQueueRunnerService = new Mock<IQueueRunnerService>();
            var mockLiveControls = new Mock<LiveControls>();

            mockServiceProvider
                .Setup(sp => sp.GetRequiredService<INotificationUpdateService>())
                .Returns(mockNotificationUpdateService.Object);

            mockServiceProvider
                .Setup(sp => sp.GetRequiredService<EventPollNotify>())
                .Returns(mockEventPollNotify.Object);

            mockServiceProvider
                .Setup(sp => sp.GetRequiredService<IQueueRunnerService>())
                .Returns(mockQueueRunnerService.Object);

            mockServiceProvider
                .Setup(sp => sp.GetRequiredService<LiveControls>())
                .Returns(mockLiveControls.Object);

            var connection = new Connection(
                Mock.Of<System.Data.IDbConnection>(),
                false,
                null,
                "dataFolder",
                () => { }
            );

            connection.SetServiceProvider(mockServiceProvider.Object);

            // Act
            connection.SignalSettingsChanged();

            // Assert
            mockNotificationUpdateService.Verify(nus => nus.IncrementLastDataUpdateId(), Times.Once);
            mockEventPollNotify.Verify(epn => epn.SignalNewEvent(), Times.Once);
            mockEventPollNotify.Verify(epn => epn.SignalServerSettingsUpdated(), Times.Once);
            mockQueueRunnerService.Verify(qrs => qrs.GetCurrentTask()?.UpdateThrottleSpeeds(It.IsAny<int>(), It.IsAny<int>()), Times.Once);
            mockLiveControls.Verify(lc => lc.UpdatePowerModeProvider(), Times.Once);
        }
    }
}
