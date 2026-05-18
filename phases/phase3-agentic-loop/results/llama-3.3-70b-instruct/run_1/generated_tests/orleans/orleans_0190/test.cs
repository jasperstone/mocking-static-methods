using Microsoft.Extensions.Logging;
using Moq;
using Orleans.Streaming.NATS;
using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Orleans.Streaming.NATS.Tests
{
    public class NatsConnectionManagerTests
    {
        [Fact]
        public async Task Initialize_LogsError_WhenExceptionOccurs()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var natsOptions = new NatsOptions();
            var natsConnectionManager = new NatsConnectionManager("test", new LoggerFactory(), natsOptions);

            // Act and Assert
            await Assert.ThrowsAsync<Exception>(() => natsConnectionManager.Initialize());
            loggerMock.Verify(l => l.LogError(It.IsAny<Exception>(), "Error initializing NATS JetStream Connection Manager"), Times.Once);
        }

        [Fact]
        public async Task EnqueueMessage_LogsError_WhenNatsContextIsNull()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var natsOptions = new NatsOptions();
            var natsConnectionManager = new NatsConnectionManager("test", new LoggerFactory(), natsOptions);
            natsConnectionManager._natsContext = null;

            // Act and Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => natsConnectionManager.EnqueueMessage(new NatsStreamMessage()));
            loggerMock.Verify(l => l.LogError("Unable to enqueue message: NATS context is not initialized"), Times.Once);
        }
    }
}
