using Xunit;
using Moq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using NATS.Client.Core;
using NATS.Client.JetStream;

namespace Orleans.Streaming.NATS.Tests
{
    public class NatsConnectionManagerTests
    {
        [Fact]
        public async Task Initialize_LogsError_WhenExceptionOccurs()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var natsConnectionMock = new Mock<NatsConnection>();
            var natsContextMock = new Mock<NatsJSContext>();
            var natsClientOptionsMock = new Mock<NatsOpts>();
            var optionsMock = new Mock<NatsOptions>();

            natsConnectionMock.Setup(c => c.ConnectAsync()).Throws(new Exception("Test exception"));

            var natsConnectionManager = new NatsConnectionManager(
                "test-provider",
                new LoggerFactory(),
                optionsMock.Object);

            // Act
            await Assert.ThrowsAsync<Exception>(() => natsConnectionManager.Initialize());

            // Assert
            loggerMock.Verify(l => l.LogError(It.IsAny<Exception>(), "Error initializing NATS JetStream Connection Manager"), Times.Once);
        }
    }
}
