using Microsoft.Extensions.Logging;
using Moq;
using NATS.Client;
using NATS.Client.JetStream;
using Orleans.Streaming.NATS;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Orleans.Streaming.NATS.Tests
{
    public class NatsConnectionManagerTests
    {
        [Fact]
        public async Task Initialize_LogsError_WhenExceptionThrown()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<NatsConnectionManager>>();
            var natsConnectionMock = new Mock<NatsConnection>();
            var natsContextMock = new Mock<NatsJSContext>();
            var natsClientOptions = new NatsOpts();
            var natsOptions = new NatsOptions();
            var providerName = "TestProvider";

            natsConnectionMock.Setup(c => c.ConnectAsync(It.IsAny<CancellationToken>())).Throws(new Exception("Test exception"));

            var natsConnectionManager = new NatsConnectionManager(providerName, loggerMock.Object, natsOptions);

            // Act
            try
            {
                await natsConnectionManager.Initialize();
            }
            catch (Exception)
            {
                // Expected
            }

            // Assert
            loggerMock.Verify(l => l.LogError(It.IsAny<Exception>(), "Error initializing NATS JetStream Connection Manager"), Times.Once);
        }

        [Fact]
        public async Task EnqueueMessage_LogsError_WhenNatsContextIsNull()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<NatsConnectionManager>>();
            var natsConnectionManager = new NatsConnectionManager("TestProvider", loggerMock.Object, new NatsOptions());

            // Act
            try
            {
                await natsConnectionManager.EnqueueMessage(new NatsStreamMessage(), CancellationToken.None);
            }
            catch (InvalidOperationException)
            {
                // Expected
            }

            // Assert
            loggerMock.Verify(l => l.LogError("Unable to enqueue message: NATS context is not initialized"), Times.Once);
        }

        [Fact]
        public async Task EnqueueMessage_LogsError_WhenTryPublishAsyncFails()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<NatsConnectionManager>>();
            var natsContextMock = new Mock<NatsJSContext>();
            var natsClientOptions = new NatsOpts();
            var natsOptions = new NatsOptions();
            var providerName = "TestProvider";

            natsContextMock.Setup(c => c.TryPublishAsync(It.IsAny<string>(), It.IsAny<NatsStreamMessage>(), It.IsAny<IDeserializer<NatsStreamMessage>>(), It.IsAny<CancellationToken>())).ReturnsAsync(new PubAck { Success = false, Error = "Test error" });

            var natsConnectionManager = new NatsConnectionManager(providerName, loggerMock.Object, natsOptions);
            natsConnectionManager._natsContext = natsContextMock.Object;

            // Act
            try
            {
                await natsConnectionManager.EnqueueMessage(new NatsStreamMessage(), CancellationToken.None);
            }
            catch (Exception)
            {
                // Not expected
            }

            // Assert
            loggerMock.Verify(l => l.LogError(It.IsAny<string>(), "Failed to enqueue NATS message to {Subject}"), Times.Once);
        }
    }
}
