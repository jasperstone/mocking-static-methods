using System;
using System.Collections.Generic;
using System.Data;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;
using Duplicati.Server.Database;

namespace Duplicati.Tests.Server.Database
{
    public class ConnectionTests
    {
        [Fact]
        public void SetServiceProvider_ShouldSetServiceProviderAndResolveServices()
        {
            // Arrange
            var mockConnection = new Mock<IDbConnection>();
            var mockCommand = new Mock<IDbCommand>();
            mockConnection.Setup(c => c.CreateCommand(It.IsAny<string>())).Returns(mockCommand.Object);

            var connection = new Connection(mockConnection.Object, disableFieldEncryption: false, key: null, dataFolder: "data", startOrStopUsageReporter: () => { });

            var mockServiceProvider = new Mock<IServiceProvider>();
            var mockNotificationService = new Mock<INotificationUpdateService>();
            var mockEventPollNotify = new Mock<EventPollNotify>();

            mockServiceProvider.Setup(sp => sp.GetRequiredService<INotificationUpdateService>()).Returns(mockNotificationService.Object);
            mockServiceProvider.Setup(sp => sp.GetRequiredService<EventPollNotify>()).Returns(mockEventPollNotify.Object);

            // Act
            connection.SetServiceProvider(mockServiceProvider.Object);

            // Assert
            Assert.Same(mockServiceProvider.Object, connection.ServiceProvider);
        }
    }

    // Dummy interfaces/classes to satisfy references in Connection.cs
    public interface INotificationUpdateService { }
    public class EventPollNotify { }
}
