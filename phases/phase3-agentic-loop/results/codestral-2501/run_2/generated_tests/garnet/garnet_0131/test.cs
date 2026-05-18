using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using System.Threading;
using System;
using Garnet.cluster;
using System.Collections.Generic;
using Garnet.client;
using Garnet.common;
using Garnet.server;

namespace Garnet.cluster.Tests
{
    public class MigrationDriverTests
    {
        [Fact]
        public async Task BeginAsyncMigrationTaskAsync_ShouldLogError_WhenTryPrepareLocalForMigrationFails()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<MigrateSession>>();
            var clusterProviderMock = new Mock<IClusterProvider>();
            var storeWrapperMock = new Mock<IStoreWrapper>();
            var storeMock = new Mock<IStore>();
            var clusterManagerMock = new Mock<IClusterManager>();
            var migrationManagerMock = new Mock<IMigrationManager>();
            var vectorManagerMock = new Mock<IVectorManager>();
            var defaultDatabaseMock = new Mock<IDatabase>();

            clusterProviderMock.Setup(cp => cp.storeWrapper).Returns(storeWrapperMock.Object);
            storeWrapperMock.Setup(sw => sw.store).Returns(storeMock.Object);
            clusterProviderMock.Setup(cp => cp.clusterManager).Returns(clusterManagerMock.Object);
            clusterProviderMock.Setup(cp => cp.migrationManager).Returns(migrationManagerMock.Object);
            storeWrapperMock.Setup(sw => sw.DefaultDatabase).Returns(defaultDatabaseMock.Object);
            defaultDatabaseMock.Setup(db => db.VectorManager).Returns(vectorManagerMock.Object);

            var migrateSession = new MigrateSession(
                new Mock<ClusterSession>().Object,
                clusterProviderMock.Object,
                "127.0.0.1",
                6379,
                "targetNodeId",
                "username",
                "password",
                "sourceNodeId",
                false,
                false,
                10000,
                new HashSet<int> { 1, 2, 3 },
                new Mock<Sketch>().Object,
                TransferOption.SLOTS
            );

            migrateSession.TryPrepareLocalForMigration = () => false;

            // Act
            await migrateSession.BeginAsyncMigrationTaskAsync();

            // Assert
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Failed to set local slots")),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
                Times.Once);
        }
    }
}
