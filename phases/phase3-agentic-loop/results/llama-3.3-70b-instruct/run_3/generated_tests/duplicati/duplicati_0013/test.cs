using Xunit;
using Moq;
using Microsoft.Extensions.DependencyInjection;
using System;
using Duplicati.Server.Database;

namespace Duplicati.Tests
{
    public class ConnectionTests
    {
        [Fact]
        public void SignalSettingsChanged_CallsIncrementLastDataUpdateId()
        {
            // Arrange
            var mockNotificationUpdateService = new Mock<Duplicati.Library.Interfaces.INotificationUpdateService>();
            var services = new ServiceCollection();
            services.AddSingleton<Duplicati.Library.Interfaces.INotificationUpdateService>(mockNotificationUpdateService.Object);
            var serviceProvider = services.BuildServiceProvider();

            var connection = new Duplicati.Server.Database.Connection(null, false, null, "", () => { });
            connection.SetServiceProvider(serviceProvider);

            // Act
            connection.SignalSettingsChanged();

            // Assert
            mockNotificationUpdateService.Verify(s => s.IncrementLastDataUpdateId(), Times.Once);
        }
    }
}
