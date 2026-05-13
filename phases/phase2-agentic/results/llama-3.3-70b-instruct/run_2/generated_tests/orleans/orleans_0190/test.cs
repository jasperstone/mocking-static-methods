using Microsoft.Extensions.Logging;
using Moq;
using NATS.Client;
using NATS.Client.JetStream;
using Orleans.Streaming.NATS;
using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Orleans.Streaming.NATS.Tests
{
    public class NatsConnectionManagerTests
    {
        private readonly Mock<ILoggerFactory> _loggerFactoryMock;
        private readonly Mock<ILogger<NatsConnectionManager>> _loggerMock;
        private readonly NatsOptions _natsOptions;

        public NatsConnectionManagerTests()
        {
            _loggerFactoryMock = new Mock<ILoggerFactory>();
            _loggerMock = new Mock<ILogger<NatsConnectionManager>>();
            _loggerFactoryMock.Setup(x => x.CreateLogger(It.IsAny<string>())).Returns(_loggerMock.Object);
            _natsOptions = new NatsOptions();
        }

        [Fact]
        public async Task Initialize_ThrowsException_LogsError()
        {
            // Arrange
            var natsConnectionManager = new NatsConnectionManager("test", _loggerFactoryMock.Object, _natsOptions);
            var exception = new Exception("Test exception");

            // Act and Assert
            await Assert.ThrowsAsync<Exception>(() => natsConnectionManager.Initialize());
            _loggerMock.Verify(x => x.LogError(It.IsAny<Exception>(), "Error initializing NATS JetStream Connection Manager"), Times.Once);
        }

        [Fact]
        public async Task EnqueueMessage_ThrowsException_LogsError()
        {
            // Arrange
            var natsConnectionManager = new NatsConnectionManager("test", _loggerFactoryMock.Object, _natsOptions);
            var message = new NatsStreamMessage();

            // Act and Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => natsConnectionManager.EnqueueMessage(message));
            _loggerMock.Verify(x => x.LogError("Unable to enqueue message: NATS context is not initialized"), Times.Once);
        }
    }
}
