using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using System.Threading;
using System.Collections.Generic;
using System;

namespace Garnet.cluster.Tests
{
    public class MigrationDriverTests
    {
        private readonly Mock<ILogger<MigrateSession>> _loggerMock;
        private readonly Mock<ClusterProvider> _clusterProviderMock;
        private readonly Mock<StoreWrapper> _storeWrapperMock;
        private readonly Mock<Store> _storeMock;
        private readonly Mock<ClusterManager> _clusterManagerMock;
        private readonly Mock<VectorManager> _vectorManagerMock;
        private readonly Mock<Database> _databaseMock;
        private readonly Mock<MigrationManager> _migrationManagerMock;
        private readonly Mock<MigrateOperation> _migrateOperationMock;
        private readonly Mock<Client> _clientMock;
        private readonly MigrateSession _migrateSession;

        public MigrationDriverTests()
        {
            _loggerMock = new Mock<ILogger<MigrateSession>>();
            _clusterProviderMock = new Mock<ClusterProvider>();
            _storeWrapperMock = new Mock<StoreWrapper>();
            _storeMock = new Mock<Store>();
            _clusterManagerMock = new Mock<ClusterManager>();
            _vectorManagerMock = new Mock<VectorManager>();
            _databaseMock = new Mock<Database>();
            _migrationManagerMock = new Mock<MigrationManager>();
            _migrateOperationMock = new Mock<MigrateOperation>();
            _clientMock = new Mock<Client>();

            _clusterProviderMock.Setup(cp => cp.storeWrapper).Returns(_storeWrapperMock.Object);
            _storeWrapperMock.Setup(sw => sw.store).Returns(_storeMock.Object);
            _storeWrapperMock.Setup(sw => sw.DefaultDatabase).Returns(_databaseMock.Object);
            _databaseMock.Setup(db => db.VectorManager).Returns(_vectorManagerMock.Object);
            _clusterProviderMock.Setup(cp => cp.clusterManager).Returns(_clusterManagerMock.Object);
            _clusterProviderMock.Setup(cp => cp.migrationManager).Returns(_migrationManagerMock.Object);
            _migrateOperationMock.Setup(mo => mo.Client).Returns(_clientMock.Object);

            _migrateSession = new MigrateSession(_loggerMock.Object, _clusterProviderMock.Object, new CancellationTokenSource(), TimeSpan.FromSeconds(10), new List<MigrateOperation> { _migrateOperationMock.Object });
        }

        [Fact]
        public async Task BeginAsyncMigrationTaskAsync_ShouldLogError_WhenTryPrepareLocalForMigrationFails()
        {
            // Arrange
            _clientMock.Setup(c => c.SetSlotRange(It.IsAny<byte[]>(), It.IsAny<string>(), It.IsAny<List<SlotRange>>())).ReturnsAsync("OK");
            _vectorManagerMock.Setup(vm => vm.GetNamespacesForHashSlots(It.IsAny<List<int>>())).Returns(new List<string>());
            _migrateSession.GetSourceNodeId = "sourceNodeId";
            _migrateSession.GetSlots = new List<int> { 1, 2, 3 };

            // Act
            await _migrateSession.BeginAsyncMigrationTaskAsync();

            // Assert
            _loggerMock.Verify(logger => logger.LogError("Failed to set local slots {slots} to migrate state", It.IsAny<object[]>()), Times.Once);
        }
    }
}
