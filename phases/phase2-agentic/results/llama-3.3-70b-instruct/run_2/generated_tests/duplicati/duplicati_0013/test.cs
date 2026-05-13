using Xunit;
using Moq;
using Duplicati.Library.Interfaces;
using Duplicati.Library.RestAPI.Database;
using Microsoft.Extensions.DependencyInjection;

namespace Duplicati.Tests
{
    public class ConnectionTests
    {
        [Fact]
        public void SignalSettingsChanged_CallsIncrementLastDataUpdateId()
        {
            // Arrange
            var serviceProviderMock = new Mock<IServiceProvider>();
            var notificationUpdateServiceMock = new Mock<INotificationUpdateService>();
            var eventPollNotifyMock = new Mock<EventPollNotify>();
            var queueRunnerServiceMock = new Mock<IQueueRunnerService>();

            serviceProviderMock
                .Setup(p => p.GetRequiredService<INotificationUpdateService>())
                .Returns(notificationUpdateServiceMock.Object);

            serviceProviderMock
                .Setup(p => p.GetRequiredService<EventPollNotify>())
                .Returns(eventPollNotifyMock.Object);

            serviceProviderMock
                .Setup(p => p.GetRequiredService<IQueueRunnerService>())
                .Returns(queueRunnerServiceMock.Object);

            var connection = new Connection(serviceProviderMock.Object);

            // Act
            connection.SignalSettingsChanged();

            // Assert
            notificationUpdateServiceMock.Verify(s => s.IncrementLastDataUpdateId(), Times.Once);
            eventPollNotifyMock.Verify(e => e.SignalNewEvent(), Times.Once);
            eventPollNotifyMock.Verify(e => e.SignalServerSettingsUpdated(), Times.Once);
            queueRunnerServiceMock.Verify(q => q.GetCurrentTask(), Times.Once);
        }
    }
}
