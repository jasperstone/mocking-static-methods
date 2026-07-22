using System;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Orleans.Streaming.NATS;
using Xunit;

namespace Orleans.Streaming.NATS.Tests
{
    public class NatsConnectionManagerInitializeTests
    {
        [Fact]
        public async Task Initialize_LogsError_WhenExceptionThrown()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var loggerFactoryMock = new Mock<ILoggerFactory>();
            loggerFactoryMock.Setup(f => f.CreateLogger(It.IsAny<string>())).Returns(loggerMock.Object);

            var optionsType = typeof(NatsOptions);
            var options = Activator.CreateInstance(optionsType);
            optionsType.GetProperty("StreamName")?.SetValue(options, "testStream");
            optionsType.GetProperty("ProducerCount")?.SetValue(options, 1);
            optionsType.GetProperty("PartitionCount")?.SetValue(options, 1);

            var natsConnectionManagerType = typeof(NatsConnectionManager);
            var ctor = natsConnectionManagerType.GetConstructor(
                BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public,
                null,
                new Type[] { typeof(string), typeof(ILoggerFactory), optionsType },
                null);
            Assert.NotNull(ctor);

            var manager = ctor.Invoke(new object[] { "testProvider", loggerFactoryMock.Object, options });

            // Act
            var initializeMethod = natsConnectionManagerType.GetMethod("Initialize", BindingFlags.Instance | BindingFlags.Public);
            Assert.NotNull(initializeMethod);

            var task = (Task)initializeMethod.Invoke(manager, new object[] { CancellationToken.None })!;
            Exception? caughtException = null;
            try
            {
                await task;
            }
            catch (Exception ex)
            {
                caughtException = ex;
            }

            // Assert
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Error initializing NATS JetStream Connection Manager")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.AtLeastOnce);

            Assert.NotNull(caughtException);
        }
    }
}
