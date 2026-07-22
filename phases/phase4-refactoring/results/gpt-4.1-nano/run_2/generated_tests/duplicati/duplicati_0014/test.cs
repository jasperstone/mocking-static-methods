using Xunit;
using Moq;
using System;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using Duplicati.Library.RestAPI.Database;

namespace Duplicati.Tests
{
    public class ConnectionTests
    {
        [Fact]
        public void SignalSettingsChanged_Should_Call_EventPollNotify_SignalNewEvent()
        {
            // Arrange
            var mockServiceProvider = new Mock<IServiceProvider>();
            var mockEventPollNotify = new Mock<EventPollNotify>();

            // Setup the service provider to return the mock EventPollNotify
            mockServiceProvider.Setup(sp => sp.GetRequiredService<EventPollNotify>())
                .Returns(mockEventPollNotify.Object);

            // Create a dummy IDbConnection
            var dummyConnection = new Mock<System.Data.IDbConnection>().Object;
            var connection = new Connection(dummyConnection, disableFieldEncryption: false, null, "dataFolder", () => { });

            // Set the service provider
            connection.SetServiceProvider(mockServiceProvider.Object);

            // Act
            // Call the method that triggers SignalSettingsChanged
            connection.SignalSettingsChanged();

            // Assert
            // Verify that SignalNewEvent was called once
            mockEventPollNotify.Verify(e => e.SignalNewEvent(), Times.Once);
        }
    }
}
