using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using Garnet.cluster;

namespace Garnet.Tests
{
    public class MigrationDriverTests
    {
        [Fact]
        public async Task BeginAsyncMigrationTaskAsync_Should_LogError_When_RelinquishOwnershipFails()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<MigrationDriver>>();
            var clusterProviderMock = new Mock<IClusterProvider>();
            var clusterManagerMock = new Mock<IClusterManager>();
            var storeWrapperMock = new Mock<IStoreWrapper>();
            var migrationManagerMock = new Mock<IMigrationManager>();
            var migrationDriver = new MigrationDriver(
                loggerMock.Object,
                clusterProviderMock.Object,
                migrationManagerMock.Object);

            // Setup dependencies
            clusterProviderMock.Setup(cp => cp.clusterManager).Returns(clusterManagerMock.Object);
            clusterProviderMock.Setup(cp => cp.storeWrapper).Returns(storeWrapperMock.Object);
            clusterProviderMock.Setup(cp => cp.migrationManager).Returns(migrationManagerMock.Object);
            // Setup methods
            migrationDriver.GetSourceNodeId = "sourceNodeId";
            migrationDriver.GetTargetNodeId = "targetNodeId";
            migrationDriver.GetTargetEndpoint = "endpoint";
            migrationDriver.GetSlots = new[] { 1, 2, 3 };
            migrationDriver.RelinquishOwnership = () => false; // Force failure
            migrationDriver.Status = MigrateState.IN_PROGRESS;

            // Mock other methods to do nothing
            migrationDriver.TrySetSlotRangesAsync = (nodeId, state) => Task.FromResult(true);
            migrationDriver.clusterProvider = clusterProviderMock.Object;
            migrationDriver._sslots = new[] { 1, 2, 3 };
            migrationDriver._timeout = TimeSpan.FromSeconds(30);
            migrationDriver._cts = new CancellationTokenSource();

            // Act
            await migrationDriver.BeginAsyncMigrationTaskAsync();

            // Assert
            // Verify that LogError was called with the expected message
            loggerMock.Verify(
                x => x.LogError(It.IsAny<string>()),
                Times.AtLeastOnce);
        }
    }
}
