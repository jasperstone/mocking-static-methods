using Xunit;
using Moq;
using System;
using Duplicati.Library.Interface;
using Microsoft.Extensions.DependencyInjection;
using Duplicati.Server.Database;

public class ConnectionTests
{
    [Fact]
    public void SignalSettingsChanged_ResolvesINotificationUpdateService()
    {
        // Arrange
        var serviceProviderMock = new Mock<IServiceProvider>();
        var notificationUpdateServiceMock = new Mock<INotificationUpdateService>();
        serviceProviderMock.Setup(p => p.GetRequiredService<INotificationUpdateService>()).Returns(notificationUpdateServiceMock.Object);

        var connection = new Connection(null, false, null, string.Empty, () => { });
        connection.SetServiceProvider(serviceProviderMock.Object);

        // Act
        connection.SignalSettingsChanged();

        // Assert
        notificationUpdateServiceMock.Verify(s => s.IncrementLastDataUpdateId(), Times.Once);
    }
}
