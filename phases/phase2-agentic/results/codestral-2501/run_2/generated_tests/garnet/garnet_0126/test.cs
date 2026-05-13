using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Garnet.cluster.Tests
{
    public class MigrationDriverTests
    {
        [Fact]
        public async Task TrySetSlotRangesAsync_ShouldLogErrorAndReturnFalse_WhenSetSlotRangeFails()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<MigrateSession>>();
            var migrateSession = new MigrateSession(loggerMock.Object);

            var clientMock = new Mock<IMigrateClient>();
            clientMock.Setup(client => client.SetSlotRange(It.IsAny<byte>(), It.IsAny<string>(), It.IsAny<SlotRange[]>()))
                .ReturnsAsync("ERROR");

            migrateSession.migrateOperation = new[] { new MigrateOperation { Client = clientMock.Object } };
            migrateSession._timeout = TimeSpan.FromMilliseconds(1000);
            migrateSession._cts = new CancellationTokenSource();
            migrateSession._sslots = new[] { 1, 2, 3 };

            // Act
            var result = await migrateSession.TrySetSlotRangesAsync("nodeid", MigrateState.IMPORT);

            // Assert
            loggerMock.Verify(logger => logger.LogError(It.IsAny<EventId>(), It.IsAny<It.IsAnyType>(), It.IsAny<It.IsAnyType>(), It.IsAny<It.IsAnyType>(), It.IsAny<It.IsAnyType>()), Times.Once);
            Assert.False(result);
            Assert.Equal(MigrateState.FAIL, migrateSession.Status);
        }

        [Fact]
        public async Task TrySetSlotRangesAsync_ShouldLogErrorAndReturnFalse_WhenOperationIsCancelled()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<MigrateSession>>();
            var migrateSession = new MigrateSession(loggerMock.Object);

            var clientMock = new Mock<IMigrateClient>();
            clientMock.Setup(client => client.SetSlotRange(It.IsAny<byte>(), It.IsAny<string>(), It.IsAny<SlotRange[]>()))
                .ThrowsAsync(new OperationCanceledException());

            migrateSession.migrateOperation = new[] { new MigrateOperation { Client = clientMock.Object } };
            migrateSession._timeout = TimeSpan.FromMilliseconds(1000);
            migrateSession._cts = new CancellationTokenSource();
            migrateSession._sslots = new[] { 1, 2, 3 };

            // Act
            var result = await migrateSession.TrySetSlotRangesAsync("nodeid", MigrateState.IMPORT);

            // Assert
            loggerMock.Verify(logger => logger.LogError(It.IsAny<EventId>(), It.IsAny<It.IsAnyType>(), It.IsAny<It.IsAnyType>(), It.IsAny<It.IsAnyType>(), It.IsAny<It.IsAnyType>()), Times.Once);
            Assert.False(result);
            Assert.Equal(MigrateState.FAIL, migrateSession.Status);
        }

        [Fact]
        public async Task TrySetSlotRangesAsync_ShouldLogErrorAndReturnFalse_WhenExceptionIsThrown()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<MigrateSession>>();
            var migrateSession = new MigrateSession(loggerMock.Object);

            var clientMock = new Mock<IMigrateClient>();
            clientMock.Setup(client => client.SetSlotRange(It.IsAny<byte>(), It.IsAny<string>(), It.IsAny<SlotRange[]>()))
                .ThrowsAsync(new Exception("Test exception"));

            migrateSession.migrateOperation = new[] { new MigrateOperation { Client = clientMock.Object } };
            migrateSession._timeout = TimeSpan.FromMilliseconds(1000);
            migrateSession._cts = new CancellationTokenSource();
            migrateSession._sslots = new[] { 1, 2, 3 };

            // Act
            var result = await migrateSession.TrySetSlotRangesAsync("nodeid", MigrateState.IMPORT);

            // Assert
            loggerMock.Verify(logger => logger.LogError(It.IsAny<EventId>(), It.IsAny<It.IsAnyType>(), It.IsAny<It.IsAnyType>(), It.IsAny<It.IsAnyType>(), It.IsAny<It.IsAnyType>()), Times.Once);
            Assert.False(result);
            Assert.Equal(MigrateState.FAIL, migrateSession.Status);
        }
    }
}
