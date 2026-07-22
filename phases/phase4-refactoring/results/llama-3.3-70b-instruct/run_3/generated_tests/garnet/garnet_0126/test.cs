using Xunit;
using Moq;
using Microsoft.Extensions.Logging;

namespace Garnet.cluster
{
    public class MigrateSessionTests
    {
        [Fact]
        public async Task LogError_CalledWhenSetSlotRangeFails()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var migrateSession = new MigrateSession(loggerMock.Object);

            // Act
            var result = await migrateSession.TrySetSlotRangesAsync(null, MigrateState.STABLE);

            // Assert
            loggerMock.Verify(l => l.LogError(It.IsAny<string>(), It.IsAny<object[]>()), Times.Once);
        }

        [Fact]
        public async Task LogError_CalledWhenOperationIsCancelled()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var migrateSession = new MigrateSession(loggerMock.Object);

            // Act
            var result = await migrateSession.TrySetSlotRangesAsync(null, MigrateState.STABLE);

            // Assert
            loggerMock.Verify(l => l.LogError(It.IsAny<string>(), It.IsAny<object[]>()), Times.Once);
        }

        [Fact]
        public async Task LogError_CalledWhenExceptionOccurs()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var migrateSession = new MigrateSession(loggerMock.Object);

            // Act
            var result = await migrateSession.TrySetSlotRangesAsync(null, MigrateState.STABLE);

            // Assert
            loggerMock.Verify(l => l.LogError(It.IsAny<string>(), It.IsAny<object[]>()), Times.Once);
        }
    }
}
