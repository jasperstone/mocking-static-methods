using Xunit;
using Moq;
using System;
using Microsoft.Extensions.DependencyInjection;
using Duplicati.Server.Database;
using Duplicati.WebserverCore.Abstractions;

namespace Duplicati.Tests
{
    public class ConnectionTests
    {
        [Fact]
        public void SignalSettingsChanged_CallsSignalNewEvent()
        {
            // Arrange
            var mockServiceProvider = new Mock<IServiceProvider>();
            var mockEventPollNotify = new Mock<EventPollNotify>();

            mockServiceProvider
                .Setup(sp => sp.GetRequiredService<EventPollNotify>())
                .Returns(mockEventPollNotify.Object);

            var connection = new Connection(
                Mock.Of<System.Data.IDbConnection>(),
                false,
                null,
                "dataFolder",
                () => { }
            );

            connection.SetServiceProvider(mockServiceProvider.Object);

            // Act
            connection.SignalSettingsChanged();

            // Assert
            mockEventPollNotify.Verify(epn => epn.SignalNewEvent(), Times.Once);
        }
    }
}
