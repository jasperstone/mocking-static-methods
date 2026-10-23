using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using System.Threading;
using System;
using System.Collections.Generic;
using Garnet.cluster;

namespace Garnet.cluster.Tests
{
    public class MigrateSessionTests
    {
        [Fact]
        public async Task BeginAsyncMigrationTaskAsync_LogError_WhenTryPrepareLocalForMigrationFails()
        {
            // Arrange
            var mockLogger = new Mock<ILogger<MigrateSession>>();
            var mockClusterProvider = new Mock<ClusterProvider>();
            var mockStoreWrapper = new Mock<StoreWrapper>();
            var mockStore = new Mock<Store>();
            var mockClusterManager = new Mock<ClusterManager>();
            var mockVectorManager = new Mock<VectorManager>();

            mockStoreWrapper.Setup(sw => sw.store).Returns(mockStore.Object);
            mockClusterProvider.Setup(cp => cp.storeWrapper).Returns(mockStoreWrapper.Object);
            mockClusterProvider.Setup(cp => cp.clusterManager).Returns(mockClusterManager.Object);
            mockClusterProvider.Setup(cp => cp.storeWrapper.DefaultDatabase.VectorManager).Returns(mockVectorManager.Object);

            var migrateSession = new MigrateSession(
                new ClusterSession(),
                mockClusterProvider.Object,
                "targetAddress",
                1234,
                "targetNodeId",
                "username",
                "password",
                "sourceNodeId",
                false,
                false,
                10000,
                new HashSet<int> { 1, 2, 3 },
                null,
                TransferOption.SLOTS
            );

            migrateSession._timeout = TimeSpan.FromSeconds(10);
            migrateSession._cts = new CancellationTokenSource();

            // Setup the failure scenario
            migrateSession.TryPrepareLocalForMigration = () => false;

            // Act
            await migrateSession.BeginAsyncMigrationTaskAsync();

            // Assert
            mockLogger.Verify(
                x => x.LogError(
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }
    }
}
