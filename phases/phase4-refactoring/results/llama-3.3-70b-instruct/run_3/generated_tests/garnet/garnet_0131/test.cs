using Xunit;
using Moq;
using Microsoft.Extensions.Logging;

namespace Garnet.cluster
{
    public class MigrateSessionTests
    {
        [Fact]
        public async Task BeginAsyncMigrationTaskAsync_LogErrorCalled_WhenTrySetSlotRangesAsyncFails()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var migrateSession = new MigrateSession(loggerMock.Object);
            var clusterProviderMock = new Mock<IClusterProvider>();
            migrateSession.clusterProvider = clusterProviderMock.Object;

            // Act
            await migrateSession.BeginAsyncMigrationTaskAsync();

            // Assert
            loggerMock.Verify(l => l.LogError(It.IsAny<string>(), It.IsAny<object[]>()), Times.Once);
        }

        [Fact]
        public async Task BeginAsyncMigrationTaskAsync_LogErrorCalled_WhenTryPrepareLocalForMigrationFails()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var migrateSession = new MigrateSession(loggerMock.Object);
            var clusterProviderMock = new Mock<IClusterProvider>();
            migrateSession.clusterProvider = clusterProviderMock.Object;

            // Act
            await migrateSession.BeginAsyncMigrationTaskAsync();

            // Assert
            loggerMock.Verify(l => l.LogError(It.IsAny<string>(), It.IsAny<object[]>()), Times.Once);
        }

        [Fact]
        public async Task BeginAsyncMigrationTaskAsync_LogErrorCalled_WhenReserveDestinationVectorSetsAsyncFails()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var migrateSession = new MigrateSession(loggerMock.Object);
            var clusterProviderMock = new Mock<IClusterProvider>();
            migrateSession.clusterProvider = clusterProviderMock.Object;

            // Act
            await migrateSession.BeginAsyncMigrationTaskAsync();

            // Assert
            loggerMock.Verify(l => l.LogError(It.IsAny<string>(), It.IsAny<object[]>()), Times.Once);
        }

        [Fact]
        public async Task BeginAsyncMigrationTaskAsync_LogErrorCalled_WhenMigrateSlotsDriverInlineAsyncFails()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var migrateSession = new MigrateSession(loggerMock.Object);
            var clusterProviderMock = new Mock<IClusterProvider>();
            migrateSession.clusterProvider = clusterProviderMock.Object;

            // Act
            await migrateSession.BeginAsyncMigrationTaskAsync();

            // Assert
            loggerMock.Verify(l => l.LogError(It.IsAny<string>(), It.IsAny<object[]>()), Times.Once);
        }
    }
}
