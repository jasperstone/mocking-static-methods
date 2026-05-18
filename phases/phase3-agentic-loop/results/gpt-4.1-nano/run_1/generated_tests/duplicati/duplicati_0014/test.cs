using System;
using Xunit;
using Moq;
using Duplicati.Server.Database;
using Microsoft.Extensions.DependencyInjection;
using System.Data;

namespace Duplicati.Tests
{
    public class ConnectionTests
    {
        [Fact]
        public void SignalSettingsChanged_ShouldCallRequiredServices_WhenProviderIsSet()
        {
            // Arrange
            var mockNotificationService = new Mock<INotificationUpdateService>();
            var mockEventPollNotify = new Mock<EventPollNotify>();
            var mockQueueRunnerService = new Mock<IQueueRunnerService>();
            var mockLiveControls = new Mock<LiveControls>();
            var mockProvider = new Mock<IServiceProvider>();

            mockProvider.Setup(p => p.GetRequiredService<INotificationUpdateService>())
                        .Returns(mockNotificationService.Object);
            mockProvider.Setup(p => p.GetRequiredService<EventPollNotify>())
                        .Returns(mockEventPollNotify.Object);
            mockProvider.Setup(p => p.GetRequiredService<IQueueRunnerService>())
                        .Returns(mockQueueRunnerService.Object);
            mockProvider.Setup(p => p.GetRequiredService<LiveControls>())
                        .Returns(mockLiveControls.Object);

            var mockConnection = new Mock<IDbConnection>();
            var mockCommand = new Mock<IDbCommand>();
            mockConnection.Setup(c => c.CreateCommand()).Returns(mockCommand.Object);

            var connection = new Connection(
                connection: mockConnection.Object,
                disableFieldEncryption: false,
                key: null,
                dataFolder: "dummyFolder",
                startOrStopUsageReporter: () => { }
            );

            // Set the service provider
            connection.SetServiceProvider(mockProvider.Object);

            // Act
            connection.SignalSettingsChanged();

            // Assert
            mockNotificationService.Verify(s => s.IncrementLastDataUpdateId(), Times.Once);
            mockEventPollNotify.Verify(e => e.SignalNewEvent(), Times.Once);
            mockEventPollNotify.Verify(e => e.SignalServerSettingsUpdated(), Times.Once);
            mockQueueRunnerService.Verify(q => q.GetCurrentTask().UpdateThrottleSpeeds(It.IsAny<long>(), It.IsAny<long>()), Times.Once);
            mockLiveControls.Verify(l => l.UpdatePowerModeProvider(), Times.Once);
        }
    }
}
