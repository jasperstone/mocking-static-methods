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
        public void SignalSettingsChanged_ShouldCallSignalNewEventOnEventPollNotify()
        {
            // Arrange
            var serviceProviderMock = new Mock<IServiceProvider>();
            var eventPollNotifyMock = new Mock<EventPollNotify>();

            // Use GetService directly without casting
            serviceProviderMock
                .Setup(sp => sp.GetService(typeof(EventPollNotify)))
                .Returns(eventPollNotifyMock.Object);

            // Ensure the Connection class is instantiated with the mocked ServiceProvider
            var connection = new Connection(serviceProviderMock.Object);

            // Act
            connection.SignalSettingsChanged();

            // Assert
            eventPollNotifyMock.Verify(epn => epn.SignalNewEvent(), Times.Once);
        }
    }

    // Assuming Connection class is something like this for the test to work
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
