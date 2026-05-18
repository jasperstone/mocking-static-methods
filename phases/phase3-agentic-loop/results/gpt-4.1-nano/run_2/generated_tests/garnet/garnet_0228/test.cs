using System;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Garnet.cluster;
using Garnet.server;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Garnet.Tests
{
    public class ReplicationManagerTests
    {
        [Fact]
        public async Task TryReplicateDiskbasedSyncAsync_Should_LogError_When_AddressIsNull()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<ReplicationManager>>();
            var clusterManagerMock = new Mock<IClusterManager>();
            var clusterProviderMock = new Mock<IClusterProvider>();
            var storeWrapperMock = new Mock<IStoreWrapper>();
            var sessionMock = new Mock<ClusterSession>();
            var options = new ReplicateSyncOptions
            {
                NodeId = "node1",
                TryAddReplica = true,
                Force = false,
                UpgradeLock = false,
                Background = false
            };

            var config = new ClusterConfig
            {
                LocalNodeId = "localNode",
                GetLocalNodePrimaryAddress = () => (null, -1)
            };

            var clusterManager = new Mock<IClusterManager>();
            clusterManager.Setup(c => c.CurrentConfig).Returns(config);

            var clusterProvider = new Mock<IClusterProvider>();
            clusterProvider.Setup(c => c.clusterManager).Returns(clusterManager.Object);
            clusterProvider.Setup(c => c.replicationManager).Returns(new Mock<IReplicationManager>().Object);
            clusterProvider.Setup(c => c.serverOptions).Returns(new ServerOptions());
            clusterProvider.Setup(c => c.ClusterUsername).Returns("user");
            clusterProvider.Setup(c => c.ClusterPassword).Returns("pass");
            clusterProvider.Setup(c => c.clusterManager).Returns(clusterManager.Object);
            clusterProvider.Setup(c => c.clusterManager.CurrentConfig).Returns(config);
            clusterProvider.Setup(c => c.GetLatestCheckpointEntryFromDisk()).Returns(new CheckpointEntry());

            var repManager = new ReplicationManager
            {
                logger = loggerMock.Object,
                clusterProvider = clusterProvider.Object,
                storeWrapper = storeWrapperMock.Object,
                ctsRepManager = new CancellationTokenSource(),
                IsRecovering = true
            };

            // Act
            var result = await repManager.TryReplicateDiskbasedSyncAsync(sessionMock.Object, options);

            // Assert
            loggerMock.Verify(
                x => x.LogError(It.IsAny<Exception>(), It.Is<string>(s => s.Contains(nameof(repManager.TryReplicateDiskbasedSyncAsync)))),
                Times.Once);
            Assert.False(result.Success);
        }
    }

    // Dummy interfaces and classes to compile the test
    public interface IClusterManager
    {
        ClusterConfig CurrentConfig { get; }
        Task<(bool, string)> TryAddReplicaAsync(string nodeId, bool force, bool upgradeLock, ILogger logger);
    }

    public interface IClusterProvider
    {
        IClusterManager clusterManager { get; }
        IReplicationManager replicationManager { get; }
        ServerOptions serverOptions { get; }
        string ClusterUsername { get; }
        string ClusterPassword { get; }
        CheckpointEntry GetLatestCheckpointEntryFromDisk();
    }

    public interface IReplicationManager { }

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

    public class ServerOptions
    {
        public TlsOptions TlsOptions { get; set; }
        public bool EnableFastCommit { get; set; }
    }

    public class TlsOptions
    {
        public object TlsClientOptions { get; set; }
    }

    public class CheckpointEntry { }

    public class ClusterConfig
    {
        public string LocalNodeId { get; set; }
        public Func<(string, int)> GetLocalNodePrimaryAddress { get; set; }
    }

    public class ReplicateSyncOptions
    {
        public string NodeId { get; set; }
        public bool TryAddReplica { get; set; }
        public bool Force { get; set; }
        public bool UpgradeLock { get; set; }
        public bool Background { get; set; }
    }
}
