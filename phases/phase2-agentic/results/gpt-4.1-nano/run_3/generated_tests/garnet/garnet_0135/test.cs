using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Garnet.Tests.cluster
{
    public class MigrationDriverTests
    {
        private readonly Mock<ILogger<MigrationDriver>> _loggerMock;
        private readonly Mock<IClusterProvider> _clusterProviderMock;
        private readonly Mock<IMigrationManager> _migrationManagerMock;
        private readonly Mock<IClusterManager> _clusterManagerMock;
        private readonly Mock<IStoreWrapper> _storeWrapperMock;
        private readonly Mock<IStore> _storeMock;
        private readonly Mock<IVectorManager> _vectorManagerMock;
        private readonly Mock<IClient> _clientMock;

        public MigrationDriverTests()
        {
            _loggerMock = new Mock<ILogger<MigrationDriver>>();
            _clusterProviderMock = new Mock<IClusterProvider>();
            _migrationManagerMock = new Mock<IMigrationManager>();
            _clusterManagerMock = new Mock<IClusterManager>();
            _storeWrapperMock = new Mock<IStoreWrapper>();
            _storeMock = new Mock<IStore>();
            _vectorManagerMock = new Mock<IVectorManager>();
            _clientMock = new Mock<IClient>();

            // Setup default behaviors
            _clusterProviderMock.Setup(cp => cp.clusterManager).Returns(_clusterManagerMock.Object);
            _clusterProviderMock.Setup(cp => cp.storeWrapper).Returns(_storeWrapperMock.Object);
            _storeWrapperMock.Setup(sw => sw.store).Returns(_storeMock.Object);
            _storeMock.Setup(s => s.ReviveRevivification()).Verifiable();
            _clusterProviderMock.Setup(cp => cp.migrationManager).Returns(_migrationManagerMock.Object);
            _clusterProviderMock.Setup(cp => cp.BumpAndWaitForEpochTransitionAsync()).ReturnsAsync(true);
            _clusterProviderMock.Setup(cp => cp.clusterManager.SuspendConfigMerge());
            _clusterProviderMock.Setup(cp => cp.clusterManager.ResumeConfigMerge());
            _clusterProviderMock.Setup(cp => cp.storeWrapper.DefaultDatabase.VectorManager).Returns(_vectorManagerMock.Object);
            _vectorManagerMock.Setup(vm => vm.GetNamespacesForHashSlots(It.IsAny<int[]>())).Returns(new List<string>());

            // Setup for migration task
            var migration = new MigrationDriver(_loggerMock.Object, _clusterProviderMock.Object)
            {
                Status = MigrateState.INIT,
                GetTargetNodeId = "targetNode",
                GetSourceNodeId = "sourceNode",
                _sslots = new int[] { 1, 2, 3 },
                _targetAddress = "127.0.0.1",
                _targetPort = 6379,
                _timeout = TimeSpan.FromSeconds(10),
                _cts = new System.Threading.CancellationTokenSource(),
                GetSlots = new int[] { 1, 2, 3 },
                GetTargetEndpoint = "127.0.0.1:6379",
                transferOption = TransferOption.SLOTS,
            };

            // Setup for client mock
            _clientMock.Setup(c => c.SetSlotRange(It.IsAny<byte[]>(), It.IsAny<string>(), It.IsAny<int[]>()))
                .ReturnsAsync("OK");
            // Assign the mock client to the migration object if needed
        }

        [Fact]
        public async Task BeginAsyncMigrationTaskAsync_Should_LogError_When_RelinquishOwnershipFails()
        {
            // Arrange
            var migration = new MigrationDriver(_loggerMock.Object, _clusterProviderMock.Object);
            migration.Status = MigrateState.INIT;
            migration._sslots = new int[] { 1, 2, 3 };
            migration._targetAddress = "127.0.0.1";
            migration._targetPort = 6379;
            migration._timeout = TimeSpan.FromSeconds(10);
            migration._cts = new System.Threading.CancellationTokenSource();
            migration.GetSourceNodeId = "sourceNode";
            migration.GetTargetNodeId = "targetNode";

            // Mock methods
            migration.TryPrepareLocalForMigration = () => false; // Fail to set local slots to migrate
            var loggerCalls = new List<string>();
            _loggerMock.Setup(l => l.LogError(It.IsAny<string>(), It.IsAny<object[]>()))
                .Callback<string, object[]>((msg, args) => loggerCalls.Add(msg));

            // Act
            await migration.BeginAsyncMigrationTaskAsync();

            // Assert
            Assert.Contains("Failed to set local slots", loggerCalls[0]);
            Assert.Equal(MigrateState.FAIL, migration.Status);
        }

        [Fact]
        public async Task BeginAsyncMigrationTaskAsync_Should_LogError_When_TrySetSlotRangesAsyncFails()
        {
            // Arrange
            var migration = new MigrationDriver(_loggerMock.Object, _clusterProviderMock.Object);
            migration.Status = MigrateState.INIT;
            migration._sslots = new int[] { 1, 2, 3 };
            migration._targetAddress = "127.0.0.1";
            migration._targetPort = 6379;
            migration._timeout = TimeSpan.FromSeconds(10);
            migration._cts = new System.Threading.CancellationTokenSource();
            migration.GetSourceNodeId = "sourceNode";
            migration.GetTargetNodeId = "targetNode";

            // Mock methods
            migration.TryPrepareLocalForMigration = () => true; // Succeed
            migration.TrySetSlotRangesAsync = (nodeId, state) => Task.FromResult(false); // Fail
            var loggerCalls = new List<string>();
            _loggerMock.Setup(l => l.LogError(It.IsAny<string>(), It.IsAny<object[]>()))
                .Callback<string, object[]>((msg, args) => loggerCalls.Add(msg));

            // Act
            await migration.BeginAsyncMigrationTaskAsync();

            // Assert
            Assert.Contains("Failed to set remote slots", loggerCalls[0]);
            Assert.Equal(MigrateState.FAIL, migration.Status);
        }

        [Fact]
        public async Task BeginAsyncMigrationTaskAsync_Should_LogError_When_TryRecoverFromFailureAsyncFails()
        {
            // Arrange
            var migration = new MigrationDriver(_loggerMock.Object, _clusterProviderMock.Object);
            migration.Status = MigrateState.INIT;
            migration._sslots = new int[] { 1, 2, 3 };
            migration._targetAddress = "127.0.0.1";
            migration._targetPort = 6379;
            migration._timeout = TimeSpan.FromSeconds(10);
            migration._cts = new System.Threading.CancellationTokenSource();
            migration.GetSourceNodeId = "sourceNode";
            migration.GetTargetNodeId = "targetNode";

            // Mock methods
            migration.TryPrepareLocalForMigration = () => true; // Succeed
            migration.TrySetSlotRangesAsync = (nodeId, state) => Task.FromResult(false); // Fail
            migration.TryRecoverFromFailureAsync = () => Task.FromResult(false); // Fail recovery
            var loggerCalls = new List<string>();
            _loggerMock.Setup(l => l.LogError(It.IsAny<string>(), It.IsAny<object[]>()))
                .Callback<string, object[]>((msg, args) => loggerCalls.Add(msg));

            // Act
            await migration.BeginAsyncMigrationTaskAsync();

            // Assert
            Assert.Contains("Failed to set remote slots", loggerCalls[0]);
            Assert.Equal(MigrateState.FAIL, migration.Status);
        }

        [Fact]
        public async Task BeginAsyncMigrationTaskAsync_Should_SetStatusToFail_When_TryPrepareLocalForMigrationFails()
        {
            // Arrange
            var migration = new MigrationDriver(_loggerMock.Object, _clusterProviderMock.Object);
            migration.Status = MigrateState.INIT;
            migration._sslots = new int[] { 1, 2, 3 };
            migration._targetAddress = "127.0.0.1";
            migration._targetPort = 6379;
            migration._timeout = TimeSpan.FromSeconds(10);
            migration._cts = new System.Threading.CancellationTokenSource();
            migration.GetSourceNodeId = "sourceNode";
            migration.GetTargetNodeId = "targetNode";

            // Mock methods
            migration.TryPrepareLocalForMigration = () => false; // Fail
            var loggerCalls = new List<string>();
            _loggerMock.Setup(l => l.LogError(It.IsAny<string>(), It.IsAny<object[]>()))
                .Callback<string, object[]>((msg, args) => loggerCalls.Add(msg));

            // Act
            await migration.BeginAsyncMigrationTaskAsync();

            // Assert
            Assert.Contains("Failed to set local slots", loggerCalls[0]);
            Assert.Equal(MigrateState.FAIL, migration.Status);
        }
    }
}
