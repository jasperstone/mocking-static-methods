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
        [Fact]
        public async Task Initialize_ThrowsException_WhenNatsConnectionFails()
        {
            // Arrange
            var loggerFactory = new LoggerFactory();
            var logger = loggerFactory.CreateLogger<NatsConnectionManager>();
            var natsOptions = new NatsOptions();
            var natsClientOptions = new NatsOpts();
            var natsConnection = new Mock<NatsConnection>();
            natsConnection.Setup(c => c.ConnectAsync()).Throws(new Exception("Connection failed"));
            var natsContext = new Mock<NatsJSContext>();
            natsContext.Setup(c => c.Connection).Returns(natsConnection.Object);
            var natsConnectionManager = new NatsConnectionManager("test", loggerFactory, natsOptions);

            // Act and Assert
            await Assert.ThrowsAsync<Exception>(() => natsConnectionManager.Initialize());
        }

        [Fact]
        public async Task Initialize_LogsError_WhenNatsConnectionFails()
        {
            // Arrange
            var loggerFactory = new LoggerFactory();
            var logger = loggerFactory.CreateLogger<NatsConnectionManager>();
            var natsOptions = new NatsOptions();
            var natsClientOptions = new NatsOpts();
            var natsConnection = new Mock<NatsConnection>();
            natsConnection.Setup(c => c.ConnectAsync()).Throws(new Exception("Connection failed"));
            var natsContext = new Mock<NatsJSContext>();
            natsContext.Setup(c => c.Connection).Returns(natsConnection.Object);
            var natsConnectionManager = new NatsConnectionManager("test", loggerFactory, natsOptions);
            var loggerMock = new Mock<ILogger<NatsConnectionManager>>();
            natsConnectionManager._logger = loggerMock.Object;

            // Act
            try
            {
                await natsConnectionManager.Initialize();
            }
            catch
            {
            }

            // Assert
            loggerMock.Verify(l => l.LogError(It.IsAny<Exception>(), "Error initializing NATS JetStream Connection Manager"), Times.Once);
        }

        [Fact]
        public async Task EnqueueMessage_ThrowsException_WhenNatsContextIsNull()
        {
            // Arrange
            var loggerFactory = new LoggerFactory();
            var logger = loggerFactory.CreateLogger<NatsConnectionManager>();
            var natsOptions = new NatsOptions();
            var natsClientOptions = new NatsOpts();
            var natsConnection = new Mock<NatsConnection>();
            var natsContext = new Mock<NatsJSContext>();
            natsContext.Setup(c => c.Connection).Returns(natsConnection.Object);
            var natsConnectionManager = new NatsConnectionManager("test", loggerFactory, natsOptions);
            natsConnectionManager._natsContext = null;

            // Act and Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => natsConnectionManager.EnqueueMessage(new NatsStreamMessage()));
        }

        [Fact]
        public async Task EnqueueMessage_LogsError_WhenNatsContextIsNull()
        {
            // Arrange
            var loggerFactory = new LoggerFactory();
            var logger = loggerFactory.CreateLogger<NatsConnectionManager>();
            var natsOptions = new NatsOptions();
            var natsClientOptions = new NatsOpts();
            var natsConnection = new Mock<NatsConnection>();
            var natsContext = new Mock<NatsJSContext>();
            natsContext.Setup(c => c.Connection).Returns(natsConnection.Object);
            var natsConnectionManager = new NatsConnectionManager("test", loggerFactory, natsOptions);
            natsConnectionManager._natsContext = null;
            var loggerMock = new Mock<ILogger<NatsConnectionManager>>();
            natsConnectionManager._logger = loggerMock.Object;

            // Act
            try
            {
                await natsConnectionManager.EnqueueMessage(new NatsStreamMessage());
            }
            catch
            {
            }

            // Assert
            loggerMock.Verify(l => l.LogError("Unable to enqueue message: NATS context is not initialized"), Times.Once);
        }
    }
}
