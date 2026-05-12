using Xunit;
using Moq;
using System;
using Microsoft.Extensions.DependencyInjection;

namespace Duplicati.Tests
{
    public class ConnectionTests
    {
        [Fact]
        public void SetServiceProvider_GetRequiredServiceCalled()
        {
            // Arrange
            var serviceProviderMock = new Mock<IServiceProvider>();
            var notificationUpdateServiceMock = new Mock<INotificationUpdateService>();
            var eventPollNotifyerMock = new Mock<EventPollNotify>();

            serviceProviderMock
                .Setup(sp => sp.GetRequiredService<INotificationUpdateService>())
                .Returns(notificationUpdateServiceMock.Object);

            serviceProviderMock
                .Setup(sp => sp.GetRequiredService<EventPollNotify>())
                .Returns(eventPollNotifyerMock.Object);

            var connection = new Connection(null, false, null, string.Empty, () => { });

            // Act
            connection.SetServiceProvider(serviceProviderMock.Object);

            // Assert
            serviceProviderMock.Verify(sp => sp.GetRequiredService<INotificationUpdateService>(), Times.Once);
            serviceProviderMock.Verify(sp => sp.GetRequiredService<EventPollNotify>(), Times.Once);
        }
    }
}
