using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using NATS.Client.Core;
using NATS.Client.JetStream;
using NATS.Client.JetStream.Models;
using NATS.Client.Serializers.Json;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Orleans.Streaming.NATS.Tests
{
    public class NatsConnectionManagerTests
    {
        [Fact]
        public async Task Initialize_ShouldLogError_WhenConnectionFails()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<NatsConnectionManager>>();
            var natsConnectionMock = new Mock<NatsConnection>();
            var natsContextMock = new Mock<NatsJSContext>(natsConnectionMock.Object);
            var natsOptions = new NatsOptions
            {
                NatsClientOptions = NatsOpts.Default,
                ProducerCount = 1
            };

            natsConnectionMock.Setup(x => x.ConnectAsync(It.IsAny<CancellationToken>()))
                .ThrowsAsync(new Exception("Connection failed"));

            var manager = new NatsConnectionManager("testProvider", Mock.Of<ILoggerFactory>(), natsOptions);

            // Act
            await manager.Initialize(CancellationToken.None);

            // Assert
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Error initializing NATS JetStream Connection Manager")),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)!),
                Times.Once);
        }

        [Fact]
        public async Task EnqueueMessage_ShouldLogError_WhenNatsContextIsNull()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<NatsConnectionManager>>();
            var natsOptions = new NatsOptions
            {
                NatsClientOptions = NatsOpts.Default,
                ProducerCount = 1
            };

            var manager = new NatsConnectionManager("testProvider", Mock.Of<ILoggerFactory>(), natsOptions);

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => manager.EnqueueMessage(new NatsStreamMessage(), CancellationToken.None));

            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Unable to enqueue message: NATS context is not initialized")),
                    null,
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)!),
                Times.Once);
        }
    }
}
