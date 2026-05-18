using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Garnet.cluster
{
    public class MigrationDriverTests
    {
        [Fact]
        public async Task TrySetSlotRangesAsync_LogsError_WhenSetSlotRangeFails()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var clientMock = new Mock<IClusterClient>();
            clientMock.Setup(c => c.SetSlotRange(It.IsAny<byte[]>(), It.IsAny<string>(), It.IsAny<int[]>()))
                .ReturnsAsync("ERROR");

            var migrationDriver = new MigrateSession(loggerMock.Object, clientMock.Object);

            // Act
            var result = await migrationDriver.TrySetSlotRangesAsync("nodeId", MigrateState.IMPORT);

            // Assert
            loggerMock.Verify(l => l.LogError("SetSlotRange error: {error}", "ERROR"), Times.Once);
            Assert.False(result);
        }

        [Fact]
        public async Task TrySetSlotRangesAsync_LogsError_WhenOperationIsCancelled()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var clientMock = new Mock<IClusterClient>();
            clientMock.Setup(c => c.SetSlotRange(It.IsAny<byte[]>(), It.IsAny<string>(), It.IsAny<int[]>()))
                .Throws(new OperationCanceledException());

            var migrationDriver = new MigrateSession(loggerMock.Object, clientMock.Object);

            // Act
            var result = await migrationDriver.TrySetSlotRangesAsync("nodeId", MigrateState.IMPORT);

            // Assert
            loggerMock.Verify(l => l.LogError("SetSlotRange operation timed out or was cancelled after {timeout}ms for slots {slots}", It.IsAny<double>(), It.IsAny<int[]>()), Times.Once);
            Assert.False(result);
        }

        [Fact]
        public async Task TrySetSlotRangesAsync_LogsError_WhenExceptionOccurs()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var clientMock = new Mock<IClusterClient>();
            clientMock.Setup(c => c.SetSlotRange(It.IsAny<byte[]>(), It.IsAny<string>(), It.IsAny<int[]>()))
                .Throws(new Exception("Test exception"));

            var migrationDriver = new MigrateSession(loggerMock.Object, clientMock.Object);

            // Act
            var result = await migrationDriver.TrySetSlotRangesAsync("nodeId", MigrateState.IMPORT);

            // Assert
            loggerMock.Verify(l => l.LogError(It.IsAny<Exception>(), "An error occurred during SetSlotRange for slots {slots}", It.IsAny<int[]>()), Times.Once);
            Assert.False(result);
        }
    }
}
