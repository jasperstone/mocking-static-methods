using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Garnet.cluster.Tests
{
    public class MigrationDriverTests
    {
        [Fact]
        public async Task BeginAsyncMigrationTaskAsync_LogsError_WhenTryPrepareLocalForMigrationFails()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var clusterProviderMock = new Mock<ClusterProvider>();
            var migrateSession = new MigrateSession
            {
                logger = loggerMock.Object,
                clusterProvider = clusterProviderMock.Object,
                _sslots = new[] { 0, 1, 2, 3 },
                Status = MigrateState.SUCCESS
            };

            // Mock the method to return false
            migrateSession.TryPrepareLocalForMigration = () => false;

            // Act
            await migrateSession.BeginAsyncMigrationTaskAsync();

            // Assert
            loggerMock.Verify(
                x => x.LogError(
                    It.Is<string>(s => s == "Failed to set local slots 0,1,2,3 to migrate state"),
                    It.IsAny<object[]>()),
                Times.Once);
        }
    }
}
