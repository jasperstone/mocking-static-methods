using Xunit;
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Duplicati.Library.RestAPI.Database;
using Moq;

namespace Duplicati.Tests
{
    public class ConnectionTests
    {
        [Fact]
        public void SignalSettingsChanged_Should_Call_GetRequiredService_ForExpectedServices()
        {
            // Arrange
            var mockNotificationService = new Mock<INotificationUpdateService>();
            var mockEventPollNotify = new Mock<EventPollNotify>();
            var mockQueueRunnerService = new Mock<IQueueRunnerService>();
            var mockLiveControls = new Mock<LiveControls>();

            var serviceCollection = new ServiceCollection();
            serviceCollection.AddTransient<INotificationUpdateService>(_ => mockNotificationService.Object);
            serviceCollection.AddTransient<EventPollNotify>(_ => mockEventPollNotify.Object);
            serviceCollection.AddTransient<IQueueRunnerService>(_ => mockQueueRunnerService.Object);
            serviceCollection.AddTransient<LiveControls>(_ => mockLiveControls.Object);

            var serviceProvider = serviceCollection.BuildServiceProvider();

            var connection = new Connection(
                connection: new Mock<IDbConnection>().Object,
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
            mockQueueRunnerService.Verify(q => q.GetCurrentTask().UpdateThrottleSpeeds(It.IsAny<long>(), It.IsAny<long>()), Times.Once);
            mockLiveControls.Verify(l => l.UpdatePowerModeProvider(), Times.Once);
        }
    }
}
