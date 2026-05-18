using System;
using Xunit;
using Moq;
using Microsoft.Extensions.DependencyInjection;
using Duplicati.Server;

namespace Duplicati.Tests
{
    public class ConnectionTests
    {
        [Fact]
        public void SignalSettingsChanged_ShouldCallSignalNewEvent()
        {
            // Arrange
            var mockServiceProvider = new Mock<IServiceProvider>();
            var eventPollNotifyMock = new Mock<EventPollNotify>();

            mockServiceProvider
                .Setup(sp => ((Func<IServiceProvider, EventPollNotify>)ServiceProviderServiceExtensions.GetRequiredService<EventPollNotify>)(sp))
                .Returns(eventPollNotifyMock.Object);

            var connection = new Connection(mockServiceProvider.Object);

            // Act
            connection.SignalSettingsChanged();

            // Assert
            eventPollNotifyMock.Verify(epn => epn.SignalNewEvent(), Times.Once);
        }
    }

    // Mocking the Connection class for testing purposes
    public class Connection
    {
        private readonly IServiceProvider ServiceProvider;

        public Connection(IServiceProvider serviceProvider)
        {
            ServiceProvider = serviceProvider;
        }

        public void SignalSettingsChanged()
        {
            var provider = ServiceProvider;
            if (provider != null)
            {
                provider.GetRequiredService<EventPollNotify>()?.SignalNewEvent();
            }
        }
    }
}
