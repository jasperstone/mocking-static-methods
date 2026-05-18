using Xunit;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using Duplicati.Library.RestAPI.Database;
using Microsoft.Extensions.DependencyInjection;

namespace Duplicati.Tests
{
    public class ConnectionTests
    {
        [Fact]
        public void SignalSettingsChanged_ShouldCallGetRequiredServiceMethods_WhenServiceProviderIsSet()
        {
            // Arrange
            var mockNotificationService = new Mock<INotificationUpdateService>();
            var mockEventPollNotify = new Mock<EventPollNotify>();
            var mockQueueRunnerService = new Mock<IQueueRunnerService>();
            var mockCurrentTask = new Mock<IQueueRunnerService.Task>();
            var mockLiveControls = new Mock<LiveControls>();

            mockCurrentTask.Setup(t => t.UpdateThrottleSpeeds(It.IsAny<long>(), It.IsAny<long>()));

            var serviceProviderMock = new Mock<IServiceProvider>();
            serviceProviderMock.Setup(sp => sp.GetRequiredService<INotificationUpdateService>())
                .Returns(mockNotificationService.Object);
            serviceProviderMock.Setup(sp => sp.GetRequiredService<EventPollNotify>())
                .Returns(mockEventPollNotify.Object);
            serviceProviderMock.Setup(sp => sp.GetRequiredService<IQueueRunnerService>())
                .Returns(mockQueueRunnerService.Object);
            serviceProviderMock.Setup(sp => sp.GetRequiredService<LiveControls>())
                .Returns(mockLiveControls.Object);

            var connection = new Connection(
                new Mock<IDbConnection>().Object,
                disableFieldEncryption: false,
                key: null,
                dataFolder: "dummy",
                startOrStopUsageReporter: () => { }
            );

            // Set the service provider
            connection.SetServiceProvider(serviceProviderMock.Object);

            // Act
            connection.SignalSettingsChanged();

            // Assert
            mockNotificationService.Verify(s => s.IncrementLastDataUpdateId(), Times.Once);
            mockEventPollNotify.Verify(e => e.SignalNewEvent(), Times.Once);
            mockEventPollNotify.Verify(e => e.SignalServerSettingsUpdated(), Times.Once);
            mockQueueRunnerService.Verify(q => q.GetCurrentTask(), Times.Once);
            mockLiveControls.Verify(l => l.UpdatePowerModeProvider(), Times.Once);
        }
    }
}
