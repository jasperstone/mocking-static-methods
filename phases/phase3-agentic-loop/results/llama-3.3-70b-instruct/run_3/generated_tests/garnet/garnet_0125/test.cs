using Xunit;
using Moq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Garnet.cluster;

namespace Garnet.cluster
{
    public class MigrateSessionTests
    {
        [Fact]
        public async Task TrySetSlotRangesAsync_LogTrace_Called()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var migrateSession = new MigrateSession(loggerMock.Object, null, null, null, null, null, null);

            // Act
            await migrateSession.TrySetSlotRangesAsync("nodeId", MigrateState.IMPORT);

            // Assert
            loggerMock.Verify(l => l.LogTrace(It.IsAny<string>(), It.IsAny<object[]>()), Times.Once);
        }

        [Fact]
        public async Task TrySetSlotRangesAsync_LogError_Called_OnException()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var migrateSession = new MigrateSession(loggerMock.Object, null, null, null, null, null, null);

            // Act and Assert
            await Assert.ThrowsAsync<Exception>(() => migrateSession.TrySetSlotRangesAsync("nodeId", (MigrateState)100));
            loggerMock.Verify(l => l.LogError(It.IsAny<string>(), It.IsAny<object[]>()), Times.Once);
        }

        [Fact]
        public async Task TryRecoverFromFailureAsync_LogError_Called_OnFailure()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var migrateSession = new MigrateSession(loggerMock.Object, null, null, null, null, null, null);

            // Act and Assert
            await Assert.ThrowsAsync<Exception>(() => migrateSession.TryRecoverFromFailureAsync());
            loggerMock.Verify(l => l.LogError(It.IsAny<string>(), It.IsAny<object[]>()), Times.Once);
        }
    }
}
