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
        public async Task Initialize_LogsError_WhenConnectionStateNotOpen()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var loggerFactoryMock = new Mock<ILoggerFactory>();
            loggerFactoryMock.Setup(f => f.CreateLogger(It.IsAny<string>())).Returns(loggerMock.Object);
            loggerFactoryMock.Setup(f => f.CreateLogger<NatsConnectionManager>()).Returns(loggerMock.Object);

            var options = new NatsOptions
            {
                StreamName = "testStream",
                ProducerCount = 1,
                PartitionCount = 1,
                BatchSize = 1,
                NatsClientOptions = new NatsOpts
                {
                    Url = "nats://invalid:4222" // invalid URL to cause connection failure
                }
            };

            var manager = new NatsConnectionManager("testProvider", loggerFactoryMock.Object, options);

            // Act
            await manager.Initialize();

            // Assert
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Unable to connect to NATS server")),
                    null,
                    It.IsAny<Func<It.IsAnyType, System.Exception, string>>()),
                Times.AtLeastOnce);
        }
    }
}
