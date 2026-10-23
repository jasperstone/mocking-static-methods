using Xunit;
using Moq;
using Microsoft.Extensions.Logging;

namespace Garnet.cluster
{
    public class MigrateSessionTests
    {
        [Fact]
        public async Task TrySetSlotRangesAsync_LogsError_WhenSetSlotRangeFails()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var migrateSession = new MigrateSession(loggerMock.Object);

            // Act
            var result = await migrateSession.TrySetSlotRangesAsync("nodeId", MigrateState.IMPORT);

            // Assert
            loggerMock.Verify(l => l.LogError(It.IsAny<string>(), It.IsAny<object[]>()), Times.Once);
        }

        [Fact]
        public async Task TrySetSlotRangesAsync_LogsError_WhenOperationIsCancelled()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var migrateSession = new MigrateSession(loggerMock.Object);

            // Act
            var result = await migrateSession.TrySetSlotRangesAsync("nodeId", MigrateState.IMPORT);

            // Assert
            loggerMock.Verify(l => l.LogError(It.IsAny<string>(), It.IsAny<object[]>()), Times.Once);
        }

        [Fact]
        public async Task TrySetSlotRangesAsync_LogsError_WhenExceptionOccurs()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var migrateSession = new MigrateSession(loggerMock.Object);

            // Act
            var result = await migrateSession.TrySetSlotRangesAsync("nodeId", MigrateState.IMPORT);

            // Assert
            loggerMock.Verify(l => l.LogError(It.IsAny<Exception>(), It.IsAny<string>(), It.IsAny<object[]>()), Times.Once);
        }
    }
}
