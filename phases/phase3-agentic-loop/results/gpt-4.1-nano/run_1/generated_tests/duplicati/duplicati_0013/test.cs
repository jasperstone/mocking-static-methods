using System;
using Xunit;
using Moq;
using Duplicati.Library.RestAPI.Database;
using Microsoft.Extensions.DependencyInjection;
using System.Data;

namespace Duplicati.Tests
{
    public class ConnectionTests
    {
        [Fact]
        public void SignalSettingsChanged_ShouldCallRequiredServices_WhenServiceProviderIsSet()
        {
            // Arrange
            var mockNotificationService = new Mock<INotificationUpdateService>();
            var mockEventPollNotify = new Mock<EventPollNotify>();
            var mockQueueRunnerService = new Mock<IQueueRunnerService>();
            var mockTask = new Mock<IQueueRunnerService.Task>();
            var mockLiveControls = new Mock<LiveControls>();

            mockTask.Setup(t => t.UpdateThrottleSpeeds(It.IsAny<long>(), It.IsAny<long>()));

            var serviceCollection = new ServiceCollection();
            serviceCollection.AddTransient(_ => mockNotificationService.Object);
            serviceCollection.AddTransient(_ => mockEventPollNotify.Object);
            serviceCollection.AddTransient(_ => mockQueueRunnerService.Object);
            serviceCollection.AddTransient(_ => mockLiveControls.Object);

            var serviceProvider = serviceCollection.BuildServiceProvider();

            var mockConnection = new Mock<IDbConnection>();
            var mockCommand = new Mock<IDbCommand>();
            mockConnection.Setup(c => c.CreateCommand()).Returns(mockCommand.Object);

            var connection = new Connection(
                mockConnection.Object,
                disableFieldEncryption: false,
                key: null,
                dataFolder: "dummy",
                startOrStopUsageReporter: () => { }
            );

            // Set the service provider
            connection.SetServiceProvider(serviceProvider);

            // Act
            connection.SignalSettingsChanged();

            // Assert
            mockNotificationService.Verify(s => s.IncrementLastDataUpdateId(), Times.Once);
            mockEventPollNotify.Verify(e => e.SignalNewEvent(), Times.Once);
            mockEventPollNotify.Verify(e => e.SignalServerSettingsUpdated(), Times.Once);
            mockQueueRunnerService.Verify(q => q.GetCurrentTask(), Times.Once);
            mockTask.Verify(t => t.UpdateThrottleSpeeds(It.IsAny<long>(), It.IsAny<long>()), Times.Once);
            mockLiveControls.Verify(l => l.UpdatePowerModeProvider(), Times.Once);
        }
    }
}
