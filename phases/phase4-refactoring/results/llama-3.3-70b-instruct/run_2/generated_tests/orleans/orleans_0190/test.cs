using Microsoft.Extensions.Logging;
using Moq;
using Orleans.Streaming.NATS;
using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Orleans.Streaming.NATS.Tests
{
    public class TestNatsConnectionManager : NatsConnectionManager
    {
        public TestNatsConnectionManager(string providerName, ILogger logger, NatsOptions options) 
            : base(providerName, logger, options)
        {
        }

        public new Task Initialize(CancellationToken cancellationToken = default)
        {
            return base.Initialize(cancellationToken);
        }
    }

    public class NatsConnectionManagerTests
    {
        [Fact]
        public async Task Initialize_LogsError_WhenExceptionOccurs()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var natsConnectionManager = new TestNatsConnectionManager("test", loggerMock.Object, new NatsOptions());

            // Act and Assert
            await Assert.ThrowsAsync<Exception>(() => natsConnectionManager.Initialize());
            loggerMock.Verify(l => l.LogError(It.IsAny<Exception>(), "Error initializing NATS JetStream Connection Manager"), Times.Once);
        }
    }
}
