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
            var mockClient = new Mock<GarnetClientSession>();
            var migrateOperation = new MigrateOperation { Client = mockClient.Object };
            var migrateSession = new MigrateSession(migrateOperation, mockLogger.Object);

            mockClient.Setup(client => client.SetSlotRange(It.IsAny<byte[]>(), It.IsAny<string>(), It.IsAny<SlotRange[]>()))
                      .ReturnsAsync("OK");

            // Act
            var result = await migrateSession.TrySetSlotRangesAsync("nodeid", MigrateState.IMPORT);

            // Assert
            mockLogger.Verify(logger => logger.LogTrace(
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Exactly(2));
            Assert.True(result);
        }

        [Fact]
        public async Task TrySetSlotRangesAsync_LogsErrorOnFailure()
        {
            // Arrange
            var mockLogger = new Mock<ILogger<MigrateSession>>();
            var mockClient = new Mock<GarnetClientSession>();
            var migrateOperation = new MigrateOperation { Client = mockClient.Object };
            var migrateSession = new MigrateSession(migrateOperation, mockLogger.Object);

            mockClient.Setup(client => client.SetSlotRange(It.IsAny<byte[]>(), It.IsAny<string>(), It.IsAny<SlotRange[]>()))
                      .ReturnsAsync("ERROR");

            // Act
            var result = await migrateSession.TrySetSlotRangesAsync("nodeid", MigrateState.IMPORT);

            // Assert
            mockLogger.Verify(logger => logger.LogError(
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
            Assert.False(result);
        }

        [Fact]
        public async Task TrySetSlotRangesAsync_LogsErrorOnException()
        {
            // Arrange
            var mockLogger = new Mock<ILogger<MigrateSession>>();
            var mockClient = new Mock<GarnetClientSession>();
            var migrateOperation = new MigrateOperation { Client = mockClient.Object };
            var migrateSession = new MigrateSession(migrateOperation, mockLogger.Object);

            mockClient.Setup(client => client.SetSlotRange(It.IsAny<byte[]>(), It.IsAny<string>(), It.IsAny<SlotRange[]>()))
                      .ThrowsAsync(new Exception("Test Exception"));

            // Act
            var result = await migrateSession.TrySetSlotRangesAsync("nodeid", MigrateState.IMPORT);

            // Assert
            mockLogger.Verify(logger => logger.LogError(
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
            Assert.False(result);
        }

        [Fact]
        public async Task TrySetSlotRangesAsync_LogsErrorOnOperationCanceledException()
        {
            // Arrange
            var mockLogger = new Mock<ILogger<MigrateSession>>();
            var mockClient = new Mock<GarnetClientSession>();
            var migrateOperation = new MigrateOperation { Client = mockClient.Object };
            var migrateSession = new MigrateSession(migrateOperation, mockLogger.Object);

            mockClient.Setup(client => client.SetSlotRange(It.IsAny<byte[]>(), It.IsAny<string>(), It.IsAny<SlotRange[]>()))
                      .ThrowsAsync(new OperationCanceledException());

            // Act
            var result = await migrateSession.TrySetSlotRangesAsync("nodeid", MigrateState.IMPORT);

            // Assert
            mockLogger.Verify(logger => logger.LogError(
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
            Assert.False(result);
        }
    }
}
