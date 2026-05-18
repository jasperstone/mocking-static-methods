using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using Garnet.cluster;

namespace Garnet.Tests
{
    public class MigrationDriverTests
    {
        [Fact]
        public async Task TrySetSlotRangesAsync_LogsError_WhenSetSlotRangeFails()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var migrationSession = new MigrateSession(loggerMock.Object);
            var clientMock = new Mock<IClient>();
            clientMock.Setup(c => c.SetSlotRange(It.IsAny<byte[]>(), It.IsAny<string>(), It.IsAny<SlotRange[]>()))
                .ReturnsAsync("ERROR");

            // Act
            var result = await migrationSession.TrySetSlotRangesAsync("nodeId", MigrateState.IMPORT);

            // Assert
            loggerMock.Verify(l => l.LogError("SetSlotRange error: {error}", "ERROR"), Times.Once);
            Assert.False(result);
        }

        [Fact]
        public async Task TrySetSlotRangesAsync_LogsError_WhenOperationIsCancelled()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var migrationSession = new MigrateSession(loggerMock.Object);
            var clientMock = new Mock<IClient>();
            clientMock.Setup(c => c.SetSlotRange(It.IsAny<byte[]>(), It.IsAny<string>(), It.IsAny<SlotRange[]>()))
                .Throws(new OperationCanceledException());

            // Act
            var result = await migrationSession.TrySetSlotRangesAsync("nodeId", MigrateState.IMPORT);

            // Assert
            loggerMock.Verify(l => l.LogError("SetSlotRange operation timed out or was cancelled after {timeout}ms for slots {slots}", It.IsAny<double>(), It.IsAny<string>()), Times.Once);
            Assert.False(result);
        }

        [Fact]
        public async Task TrySetSlotRangesAsync_LogsError_WhenExceptionOccurs()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var migrationSession = new MigrateSession(loggerMock.Object);
            var clientMock = new Mock<IClient>();
            clientMock.Setup(c => c.SetSlotRange(It.IsAny<byte[]>(), It.IsAny<string>(), It.IsAny<SlotRange[]>()))
                .Throws(new Exception("Test exception"));

            // Act
            var result = await migrationSession.TrySetSlotRangesAsync("nodeId", MigrateState.IMPORT);

            // Assert
            loggerMock.Verify(l => l.LogError(It.IsAny<Exception>(), "An error occurred during SetSlotRange for slots {slots}", It.IsAny<string>()), Times.Once);
            Assert.False(result);
        }
    }
}
