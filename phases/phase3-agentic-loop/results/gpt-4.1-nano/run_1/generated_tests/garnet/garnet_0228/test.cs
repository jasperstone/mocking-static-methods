using System;
using System.Threading;
using System.Threading.Tasks;
using Garnet.cluster;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Garnet.Tests
{
    public class ReplicaReceiveCheckpointTests
    {
        [Fact]
        public async Task TryReplicateDiskbasedSyncAsync_Should_LogError_When_ExceptionThrown()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<ReplicationManager>>();
            var clusterProviderMock = new Mock<IClusterProvider>();
            var clusterManagerMock = new Mock<IClusterManager>();
            var replicationManagerMock = new Mock<IReplicationManager>();
            var storeWrapperMock = new Mock<IStoreWrapper>();
            var sessionMock = new Mock<ClusterSession>();
            var options = new ReplicateSyncOptions { NodeId = 1, TryAddReplica = true, Background = false, Force = false, UpgradeLock = false };
            var manager = new TestReplicationManager
            {
                Logger = loggerMock.Object,
                ClusterProvider = clusterProviderMock.Object,
                StoreWrapper = storeWrapperMock.Object,
                ClusterManager = clusterManagerMock.Object,
                ReplicationManager = replicationManagerMock.Object,
            };

            // Setup to throw exception
            manager.SetupThrowOnCall();

            // Act
            var result = await manager.TryReplicateDiskbasedSyncAsync(sessionMock.Object, options);

            // Assert
            Assert.False(result.Success);
            loggerMock.Verify(x => x.LogError(It.IsAny<Exception>(), It.Is<string>(s => s.Contains(nameof(ReplicationManager.TryReplicateDiskbasedSyncAsync)))), Times.Once);
        }

        [Fact]
        public async Task ReplicaSyncAttachTaskAsync_Should_LogError_When_PrimaryAddressNull()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<ReplicationManager>>();
            var clusterProviderMock = new Mock<IClusterProvider>();
            var clusterManagerMock = new Mock<IClusterManager>();
            var replicationManagerMock = new Mock<IReplicationManager>();
            var storeWrapperMock = new Mock<IStoreWrapper>();
            var cts = new CancellationTokenSource();

            var manager = new TestReplicationManager
            {
                Logger = loggerMock.Object,
                ClusterProvider = clusterProviderMock.Object,
                StoreWrapper = storeWrapperMock.Object,
                ClusterManager = clusterManagerMock.Object,
                ReplicationManager = replicationManagerMock.Object,
            };

            // Setup current config to return null address
            var currentConfigMock = new Mock<IClusterConfig>();
            currentConfigMock.SetupGet(c => c.GetLocalNodePrimaryAddress()).Returns((null, -1));
            clusterProviderMock.SetupGet(x => x.clusterManager.CurrentConfig).Returns(currentConfigMock.Object);

            // Act
            var result = await manager.ReplicaSyncAttachTaskAsync(downgradeLock: false, forceAsync: false);

            // Assert
            Assert.Contains("RESP_ERR_GENERIC_NOT_ASSIGNED_PRIMARY_ERROR", result);
            loggerMock.Verify(x => x.LogError("{msg}", It.Is<string>(s => s.Contains("RESP_ERR_GENERIC_NOT_ASSIGNED_PRIMARY_ERROR"))), Times.Once);
        }
    }

    // Helper classes for testing
    public class TestReplicationManager : ReplicationManager
    {
        public void SetupThrowOnCall()
        {
            // Setup to throw exception in TryReplicateDiskbasedSyncAsync
            this.TryReplicateDiskbasedSyncAsync = (session, options) =>
            {
                throw new Exception("Test exception");
            };
        }

        public new Func<ClusterSession, ReplicateSyncOptions, Task<(bool, ReadOnlyMemory<byte>)>> TryReplicateDiskbasedSyncAsync;
        public new Task<(bool, ReadOnlyMemory<byte>)> TryReplicateDiskbasedSyncAsyncMethod(ClusterSession session, ReplicateSyncOptions options)
        {
            return TryReplicateDiskbasedSyncAsync(session, options);
        }
    }

    public interface IClusterProvider
    {
        IClusterManager clusterManager { get; }
    }

    public interface IClusterManager
    {
        IClusterConfig CurrentConfig { get; }
        Task<(bool, string)> TryAddReplicaAsync(int nodeId, bool force, bool upgradeLock, ILogger logger);
    }

    public interface IClusterConfig
    {
        (string, int) GetLocalNodePrimaryAddress();
        int LocalNodeId { get; }
    }

    public interface IReplicationManager
    {
        int ReplcationOffset { get; set; }
        object GetIRSNetworkBufferSettings { get; }
        object GetNetworkPool { get; }
    }

    public interface IStoreWrapper
    {
        IAppendOnlyFile appendOnlyFile { get; }
        void Reset();
        Task SuspendPrimaryOnlyTasksAsync();
        void RecoverAOF();
    }

    public interface IAppendOnlyFile
    {
        long BeginAddress { get; }
        long TailAddress { get; }
        Task CommitAsync();
        Task WaitForCommitAsync();
    }

    public class ClusterSession
    {
        public Task UnsafeBumpAndWaitForEpochTransitionAsync() => Task.CompletedTask;
    }

    public class ReplicateSyncOptions
    {
        public int NodeId { get; set; }
        public bool TryAddReplica { get; set; }
        public bool Background { get; set; }
        public bool Force { get; set; }
        public bool UpgradeLock { get; set; }
    }
}
