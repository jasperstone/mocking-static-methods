using Xunit;
using Moq;
using Duplicati.Server;
using Microsoft.Extensions.DependencyInjection;

namespace Duplicati.Tests
{
    public class ConnectionTests
    {
        [Fact]
        public void SignalSettingsChanged_ShouldCallSignalNewEvent()
        {
            // Arrange
            var mockServiceProvider = new Mock<IServiceProvider>();
            var mockEventPollNotify = new Mock<EventPollNotify>();

            mockServiceProvider
                .Setup(sp => sp.GetRequiredService<EventPollNotify>())
                .Returns(mockEventPollNotify.Object);

            var connection = new Connection(null, false, null, "", null);
            connection.SetServiceProvider(mockServiceProvider.Object);

            // Act
            connection.SignalSettingsChanged();

            // Assert
            mockEventPollNotify.Verify(epn => epn.SignalNewEvent(), Times.Once);
        }
    }
}
