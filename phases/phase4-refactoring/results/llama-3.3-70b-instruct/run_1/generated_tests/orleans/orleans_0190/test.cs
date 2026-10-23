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
            var natsConnectionManager = new NatsConnectionManager("test", new LoggerFactory(), new NatsOptions());

            // Act and Assert
            await Assert.ThrowsAsync<Exception>(() => natsConnectionManager.Initialize());
            loggerMock.Verify(l => l.LogError(It.IsAny<Exception>(), "Error initializing NATS JetStream Connection Manager"), Times.Once);
        }
    }
}
