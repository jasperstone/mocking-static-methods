using Xunit;
using Moq;
using Duplicati.Library.Interface;
using System;
using Microsoft.Extensions.DependencyInjection;

namespace Duplicati.Tests
{
    public class ConnectionTests
    {
        [Fact]
        public void SignalSettingsChanged_IncrementsLastDataUpdateId()
        {
            // Arrange
            var notificationUpdateServiceMock = new Mock<INotificationUpdateService>();
            var serviceProvider = new ServiceCollection().AddSingleton(notificationUpdateServiceMock.Object).Build().ServiceProvider;
            var connection = new Duplicati.Server.Database.Connection(null, false, null, "", () => { });
            connection.SetServiceProvider(serviceProvider);

            // Act
            connection.SignalSettingsChanged();

            // Assert
            notificationUpdateServiceMock.Verify(s => s.IncrementLastDataUpdateId(), Times.Once);
        }
    }
}
