using Xunit;
using Moq;
using Duplicati.Server.Database;
using Duplicati.Library.Interface;

namespace Duplicati.Tests
{
    public class ConnectionTests
    {
        [Fact]
        public void TestGetRequiredService()
        {
            // Arrange
            var serviceProviderMock = new Mock<IServiceProvider>();
            var notificationUpdateServiceMock = new Mock<INotificationUpdateService>();
            serviceProviderMock.Setup(sp => sp.GetRequiredService<INotificationUpdateService>()).Returns(notificationUpdateServiceMock.Object);

            var connection = new Connection(null, false, null, null, null);
            connection.SetServiceProvider(serviceProviderMock.Object);

            // Act
            connection.SignalSettingsChanged();

            // Assert
            notificationUpdateServiceMock.Verify(nus => nus.IncrementLastDataUpdateId(), Times.Once);
        }
    }
}
