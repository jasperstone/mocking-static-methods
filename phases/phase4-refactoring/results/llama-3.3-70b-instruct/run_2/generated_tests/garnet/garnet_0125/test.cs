using Xunit;
using Moq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Garnet.cluster
{
    public class MigrationDriverTests
    {
        [Fact]
        public async Task TrySetSlotRangesAsync_LogsTrace_OnSuccess()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var migrationSession = new MigrateSession(loggerMock.Object);
            var nodeid = "nodeid";
            var state = MigrateState.STABLE;

            // Act
            var result = await migrationSession.TrySetSlotRangesAsync(nodeid, state);

            // Assert
            loggerMock.Verify(l => l.LogTrace(It.IsAny<string>(), It.IsAny<object[]>()), Times.Exactly(2));
            Assert.True(result);
        }

        [Fact]
        public async Task TrySetSlotRangesAsync_LogsError_OnFailure()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var migrationSession = new MigrateSession(loggerMock.Object);
            var nodeid = "nodeid";
            var state = MigrateState.STABLE;

            // Act
            var result = await migrationSession.TrySetSlotRangesAsync(nodeid, state);

            // Assert
            loggerMock.Verify(l => l.LogError(It.IsAny<string>(), It.IsAny<object[]>()), Times.Never);
            Assert.True(result);
        }
    }
}
