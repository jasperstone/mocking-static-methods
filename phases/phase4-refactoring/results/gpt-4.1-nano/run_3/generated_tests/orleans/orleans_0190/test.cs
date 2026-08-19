using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using Orleans.Streaming.NATS.Providers; // Adjust namespace as needed

namespace Orleans.Tests
{
    public class NatsConnectionManagerTests
    {
        [Fact]
        public async Task EnqueueMessage_NullNatsContext_LogsErrorAndThrows()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<NatsConnectionManager>>();
            var manager = new TestNatsConnectionManager(loggerMock.Object);
            manager._natsContext = null; // Simulate uninitialized context
            var message = new NatsStreamMessage
            {
                StreamId = new StreamId { Namespace = new ArraySegment<byte>(Encoding.UTF8.GetBytes("")), Key = new ArraySegment<byte>(Encoding.UTF8.GetBytes("test")) }
            };

            // Act & Assert
            var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => manager.EnqueueMessage(message));
            Assert.Equal("Unable to enqueue message: NATS context is not initialized", ex.Message);
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Unable to enqueue message: NATS context is not initialized")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        [Fact]
        public async Task EnqueueMessage_ValidContext_LogsErrorOnException()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<NatsConnectionManager>>();
            var mockContext = new Mock<INatsContext>();
            var mockSerializerRegistry = new Mock<ISerializerRegistry>();
            var mockSerializer = new Mock<ISerializer<NatsStreamMessage>>();
            mockSerializerRegistry.Setup(r => r.GetSerializer<NatsStreamMessage>()).Returns(mockSerializer.Object);
            var manager = new TestNatsConnectionManager(loggerMock.Object)
            {
                _natsContext = mockContext.Object,
                _producerNatsContexts = new[] { mockContext.Object },
                _natsClientOptions = new NatsClientOptions
                {
                    SerializerRegistry = mockSerializerRegistry.Object
                },
                _providerName = "testProvider"
            };

            var message = new NatsStreamMessage
            {
                StreamId = new StreamId
                {
                    Namespace = new ArraySegment<byte>(Encoding.UTF8.GetBytes("ns")),
                    Key = new ArraySegment<byte>(Encoding.UTF8.GetBytes("key"))
                }
            };

            mockContext.Setup(c => c.TryPublishAsync(
                It.IsAny<string>(),
                It.IsAny<NatsStreamMessage>(),
                It.IsAny<ISerializer<NatsStreamMessage>>(),
                It.IsAny<CancellationToken>()))
                .ThrowsAsync(new Exception("Publish failed"));

            // Act
            await Assert.ThrowsAsync<Exception>(() => manager.EnqueueMessage(message));

            // Assert
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Error initializing NATS JetStream Connection Manager")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        // Helper class to access protected members or to set up the class for testing
        private class TestNatsConnectionManager : NatsConnectionManager
        {
            public TestNatsConnectionManager(ILogger<NatsConnectionManager> logger) : base(logger)
            {
            }
        }
    }
}
