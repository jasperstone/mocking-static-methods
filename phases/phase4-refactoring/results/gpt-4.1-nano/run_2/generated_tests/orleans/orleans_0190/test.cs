using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using Orleans.Streaming.NATS.Providers;

namespace Orleans.Streaming.NATS.Tests
{
    public class NatsConnectionManagerTests
    {
        [Fact]
        public async Task EnqueueMessage_NullNatsContext_LogsErrorAndThrows()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<NatsConnectionManager>>();
            var manager = new NatsConnectionManager(
                loggerMock.Object,
                natsContext: null, // simulate uninitialized context
                providerName: "testProvider",
                options: new NatsOptions(),
                natsClientOptions: new NatsClientOptions(),
                producerNatsContexts: Array.Empty<INatsContext>());

            var message = new NatsStreamMessage
            {
                StreamId = new StreamId
                {
                    Namespace = ReadOnlyMemory<byte>.Empty,
                    Key = ReadOnlyMemory<byte>.Empty
                }
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
    }
}
