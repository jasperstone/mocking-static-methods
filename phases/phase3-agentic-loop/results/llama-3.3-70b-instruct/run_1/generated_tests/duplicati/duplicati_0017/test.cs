using Xunit;
using Moq;
using Microsoft.Extensions.DependencyInjection;
using Duplicati.Server.Database;

namespace Duplicati.Tests
{
    public class ConnectionTests
    {
        [Fact]
        public void SignalSettingsChanged_IncrementsLastDataUpdateId()
        {
            // Arrange
            var serviceProviderMock = new Mock<IServiceProvider>();
            var notificationUpdateServiceMock = new Mock<INotificationUpdateService>();
            serviceProviderMock.Setup(sp => sp.GetService(typeof(INotificationUpdateService))).Returns(notificationUpdateServiceMock.Object);

            var connection = new Connection(null, false, null, string.Empty, () => { });
            connection.SetServiceProvider(serviceProviderMock.Object);

            // Act
            connection.SignalSettingsChanged();

            // Assert
            notificationUpdateServiceMock.Verify(nus => nus.IncrementLastDataUpdateId(), Times.Once);
        }

        [Fact]
        public void GetRequiredService_EventPollNotify_ReturnsEventPollNotify()
        {
            // Arrange
            var serviceProviderMock = new Mock<IServiceProvider>();
            var eventPollNotifyMock = new Mock<EventPollNotify>();
            serviceProviderMock.Setup(sp => sp.GetService(typeof(EventPollNotify))).Returns(eventPollNotifyMock.Object);

            var connection = new Connection(null, false, null, string.Empty, () => { });
            connection.SetServiceProvider(serviceProviderMock.Object);

            // Act
            var eventPollNotify = connection.ServiceProvider.GetRequiredService<EventPollNotify>();

            // Assert
            Assert.NotNull(eventPollNotify);
            Assert.Equal(eventPollNotifyMock.Object, eventPollNotify);
        }
    }
}
