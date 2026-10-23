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
        private readonly Mock<ILogger<NatsConnectionManager>> _loggerMock;
        private readonly Mock<ILoggerFactory> _loggerFactoryMock;
        private readonly Mock<NatsConnection> _natsConnectionMock;
        private readonly Mock<NatsJSContext> _natsContextMock;
        private readonly NatsOptions _options;
        private readonly NatsConnectionManager _natsConnectionManager;

        public NatsConnectionManagerTests()
        {
            _loggerMock = new Mock<ILogger<NatsConnectionManager>>();
            _loggerFactoryMock = new Mock<ILoggerFactory>();
            _natsConnectionMock = new Mock<NatsConnection>();
            _natsContextMock = new Mock<NatsJSContext>(_natsConnectionMock.Object);
            _options = new NatsOptions
            {
                StreamName = "test-stream",
                PartitionCount = 1,
                ProducerCount = 1,
                BatchSize = 10,
                NatsClientOptions = new NatsOpts
                {
                    Url = "nats://localhost:4222"
                }
            };

            _loggerFactoryMock.Setup(x => x.CreateLogger<NatsConnectionManager>()).Returns(_loggerMock.Object);

            _natsConnectionManager = new NatsConnectionManager("test-provider", _loggerFactoryMock.Object, _options);
        }

        [Fact]
        public async Task Initialize_ShouldLogError_WhenConnectionFails()
        {
            // Arrange
            _natsConnectionMock.Setup(x => x.ConnectAsync(It.IsAny<CancellationToken>())).ThrowsAsync(new Exception("Connection failed"));

            // Act
            await _natsConnectionManager.Initialize(CancellationToken.None);

            // Assert
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Error initializing NATS JetStream Connection Manager")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        [Fact]
        public async Task EnqueueMessage_ShouldLogError_WhenNatsContextIsNull()
        {
            // Arrange
            var message = new NatsStreamMessage
            {
                StreamId = new StreamId("test-namespace", "test-key")
            };

            // Act
            await Assert.ThrowsAsync<InvalidOperationException>(() => _natsConnectionManager.EnqueueMessage(message, CancellationToken.None));

            // Assert
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Unable to enqueue message: NATS context is not initialized")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }
    }
}
