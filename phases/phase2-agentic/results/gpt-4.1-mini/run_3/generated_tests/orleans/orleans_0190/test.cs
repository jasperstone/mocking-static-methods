using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Orleans.Streaming.NATS;
using Xunit;

namespace Orleans.Streaming.NATS.Tests
{
    public class NatsConnectionManagerTests
    {
        [Fact]
        public async Task Initialize_WhenExceptionThrown_LogsErrorAndRethrows()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var loggerFactoryMock = new Mock<ILoggerFactory>();
            loggerFactoryMock.Setup(f => f.CreateLogger(It.IsAny<string>())).Returns(loggerMock.Object);
            loggerFactoryMock.Setup(f => f.CreateLogger(typeof(NatsConnectionManager).FullName)).Returns(loggerMock.Object);
            loggerFactoryMock.Setup(f => f.CreateLogger<NatsConnectionManager>()).Returns(loggerMock.Object);

            var options = new NatsOptions
            {
                StreamName = "testStream",
                ProducerCount = 1,
                PartitionCount = 1,
                BatchSize = 1
            };

            // We create a derived class to simulate exception on ConnectAsync
            var manager = new TestNatsConnectionManager("testProvider", loggerFactoryMock.Object, options);

            // Act & Assert
            var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => manager.Initialize(CancellationToken.None));

            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Error initializing NATS JetStream Connection Manager")),
                    ex,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        private class TestNatsConnectionManager : NatsConnectionManager
        {
            public TestNatsConnectionManager(string providerName, ILoggerFactory loggerFactory, NatsOptions options)
                : base(providerName, loggerFactory, options)
            {
            }

            public override Task Initialize(CancellationToken cancellationToken = default)
            {
                throw new InvalidOperationException("Simulated failure");
            }
        }
    }
}
