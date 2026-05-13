using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Garnet.cluster;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

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
        private readonly Mock<MigrateOperation> _migrateOperationMock;
        private readonly Mock<Client> _clientMock;
        private readonly CancellationTokenSource _cts;

        public MigrationDriverTests()
        {
            _loggerMock = new Mock<ILogger<MigrateSession>>();
            _clusterProviderMock = new Mock<ClusterProvider>();
            _storeWrapperMock = new Mock<StoreWrapper>();
            _storeMock = new Mock<Store>();
            _clusterManagerMock = new Mock<ClusterManager>();
            _vectorManagerMock = new Mock<VectorManager>();
            _databaseMock = new Mock<Database>();
            _migrateOperationMock = new Mock<MigrateOperation>();
            _clientMock = new Mock<Client>();
            _cts = new CancellationTokenSource();
        }

        [Fact]
        public async Task BeginAsyncMigrationTaskAsync_ShouldLogError_WhenTryPrepareLocalForMigrationFails()
        {
            // Arrange
            var migrationDriver = new MigrateSession(_loggerMock.Object, _clusterProviderMock.Object, _cts.Token);
            migrationDriver.GetSlots = new List<int> { 1, 2, 3 };
            migrationDriver.GetSourceNodeId = "sourceNodeId";
            migrationDriver.Status = MigrateState.PENDING;

            _clusterProviderMock.Setup(cp => cp.storeWrapper).Returns(_storeWrapperMock.Object);
            _storeWrapperMock.Setup(sw => sw.store).Returns(_storeMock.Object);
            _storeWrapperMock.Setup(sw => sw.DefaultDatabase).Returns(_databaseMock.Object);
            _databaseMock.Setup(db => db.VectorManager).Returns(_vectorManagerMock.Object);
            _clusterProviderMock.Setup(cp => cp.clusterManager).Returns(_clusterManagerMock.Object);
            _clusterProviderMock.Setup(cp => cp.migrationManager).Returns(new Mock<MigrationManager>().Object);
            _clusterProviderMock.Setup(cp => cp.BumpAndWaitForEpochTransitionAsync()).ReturnsAsync(true);
            _migrateOperationMock.Setup(mo => mo.Client).Returns(_clientMock.Object);
            migrationDriver.migrateOperation = new List<MigrateOperation> { _migrateOperationMock.Object };

            _clientMock.Setup(c => c.SetSlotRange(It.IsAny<byte[]>(), It.IsAny<string>(), It.IsAny<List<SlotRange>>()))
                .ReturnsAsync("OK");

            // Act
            await migrationDriver.BeginAsyncMigrationTaskAsync();

            // Assert
            _loggerMock.Verify(
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
