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
        public async Task TrySetSlotRangesAsync_LogTrace_Called()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var migrationDriver = new MigrationDriver(loggerMock.Object);
            var nodeid = "nodeid";
            var state = MigrateState.IMPORT;

            // Act
            await migrationDriver.TrySetSlotRangesAsync(nodeid, state);

            // Assert
            loggerMock.Verify(l => l.LogTrace(It.IsAny<string>(), It.IsAny<object[]>()), Times.Once);
        }

        [Fact]
        public async Task TrySetSlotRangesAsync_LogError_Called_When_SetSlotRange_Fails()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var migrationDriver = new MigrationDriver(loggerMock.Object);
            var nodeid = "nodeid";
            var state = MigrateState.IMPORT;

            // Act
            await migrationDriver.TrySetSlotRangesAsync(nodeid, state);

            // Assert
            loggerMock.Verify(l => l.LogError(It.IsAny<string>(), It.IsAny<object[]>()), Times.Once);
        }

        [Fact]
        public async Task TryRecoverFromFailureAsync_LogError_Called_When_TrySetSlotRangesAsync_Fails()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var migrationDriver = new MigrationDriver(loggerMock.Object);

            // Act
            await migrationDriver.TryRecoverFromFailureAsync();

            // Assert
            loggerMock.Verify(l => l.LogError(It.IsAny<string>(), It.IsAny<object[]>()), Times.Once);
        }
    }
}
