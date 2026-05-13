using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Orleans.Streaming.NATS;

namespace Orleans.Streaming.NATS.Tests
{
    public class NatsConnectionManagerTests
    {
        [Fact]
        public async Task Initialize_WhenExceptionOccurs_LogsError()
        {
            // Arrange
            var loggerFactoryMock = new Mock<ILoggerFactory>();
            var loggerMock = new Mock<ILogger<NatsConnectionManager>>();
            loggerFactoryMock.Setup(f => f.CreateLogger<NatsConnectionManager>()).Returns(loggerMock.Object);

            var options = new NatsOptions
            {
                NatsClientOptions = new NatsOpts(),
                StreamName = "test-stream",
                ProducerCount = 1,
                PartitionCount = 2
            };

            var natsConnectionManager = new NatsConnectionManager("test-provider", loggerFactoryMock.Object, options);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(() => natsConnectionManager.Initialize());

            loggerMock.Verify(
                l => l.LogError(
                    It.IsAny<Exception>(),
                    "Error initializing NATS JetStream Connection Manager",
                    It.IsAny<object[]>()),
                Times.Once);
        }
    }
}
