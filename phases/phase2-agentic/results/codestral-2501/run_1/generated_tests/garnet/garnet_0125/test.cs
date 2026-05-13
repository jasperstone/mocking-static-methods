using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using System.Threading;
using System;

namespace Garnet.cluster.Tests
{
    public class MigrationDriverTests
    {
        [Fact]
        public async Task TrySetSlotRangesAsync_LogsTraceOnSuccess()
        {
            // Arrange
            var mockLogger = new Mock<ILogger<MigrateSession>>();
            var mockClient = new Mock<IMigrateClient>();
            var migrateOperation = new MigrateOperation[] { new MigrateOperation { Client = mockClient.Object } };
            var migrateSession = new MigrateSession(migrateOperation, mockLogger.Object, new CancellationTokenSource());

            mockClient.Setup(client => client.SetSlotRange(It.IsAny<byte[]>(), It.IsAny<string>(), It.IsAny<SlotRange[]>()))
                      .ReturnsAsync("OK");

            // Act
            var result = await migrateSession.TrySetSlotRangesAsync("nodeid", MigrateState.IMPORT);

            // Assert
            mockLogger.Verify(
                logger => logger.Log(
                    LogLevel.Trace,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => true),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
                Times.Exactly(2));
            Assert.True(result);
        }

        [Fact]
        public async Task TrySetSlotRangesAsync_LogsErrorOnFailure()
        {
            // Arrange
            var mockLogger = new Mock<ILogger<MigrateSession>>();
            var mockClient = new Mock<IMigrateClient>();
            var migrateOperation = new MigrateOperation[] { new MigrateOperation { Client = mockClient.Object } };
            var migrateSession = new MigrateSession(migrateOperation, mockLogger.Object, new CancellationTokenSource());

            mockClient.Setup(client => client.SetSlotRange(It.IsAny<byte[]>(), It.IsAny<string>(), It.IsAny<SlotRange[]>()))
                      .ReturnsAsync("ERROR");

            // Act
            var result = await migrateSession.TrySetSlotRangesAsync("nodeid", MigrateState.IMPORT);

            // Assert
            mockLogger.Verify(
                logger => logger.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => true),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
                Times.Once);
            Assert.False(result);
        }

        [Fact]
        public async Task TrySetSlotRangesAsync_LogsErrorOnException()
        {
            // Arrange
            var mockLogger = new Mock<ILogger<MigrateSession>>();
            var mockClient = new Mock<IMigrateClient>();
            var migrateOperation = new MigrateOperation[] { new MigrateOperation { Client = mockClient.Object } };
            var migrateSession = new MigrateSession(migrateOperation, mockLogger.Object, new CancellationTokenSource());

            mockClient.Setup(client => client.SetSlotRange(It.IsAny<byte[]>(), It.IsAny<string>(), It.IsAny<SlotRange[]>()))
                      .ThrowsAsync(new Exception("Test exception"));

            // Act
            var result = await migrateSession.TrySetSlotRangesAsync("nodeid", MigrateState.IMPORT);

            // Assert
            mockLogger.Verify(
                logger => logger.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => true),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
                Times.Once);
            Assert.False(result);
        }

        [Fact]
        public async Task TrySetSlotRangesAsync_LogsErrorOnOperationCanceledException()
        {
            // Arrange
            var mockLogger = new Mock<ILogger<MigrateSession>>();
            var mockClient = new Mock<IMigrateClient>();
            var migrateOperation = new MigrateOperation[] { new MigrateOperation { Client = mockClient.Object } };
            var migrateSession = new MigrateSession(migrateOperation, mockLogger.Object, new CancellationTokenSource());

            mockClient.Setup(client => client.SetSlotRange(It.IsAny<byte[]>(), It.IsAny<string>(), It.IsAny<SlotRange[]>()))
                      .ThrowsAsync(new OperationCanceledException());

            // Act
            var result = await migrateSession.TrySetSlotRangesAsync("nodeid", MigrateState.IMPORT);

            // Assert
            mockLogger.Verify(
                logger => logger.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => true),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
                Times.Once);
            Assert.False(result);
        }
    }
}
