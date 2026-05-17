using System;
using System.Threading.Tasks;
using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using Garnet.cluster;
using Garnet.server;

namespace Garnet.Tests
{
    public class ReplicationManagerTests
    {
        [Fact]
        public async Task TryReplicateDiskbasedSyncAsync_Should_LogInitiatingForeground_WhenBackgroundIsFalse()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<ReplicationManager>>();
            var clusterManagerMock = new Mock<IClusterManager>();
            var clusterProviderMock = new Mock<IClusterProvider>();
            var storeWrapperMock = new Mock<IStoreWrapper>();
            var sessionMock = new Mock<ClusterSession>();

            // Setup clusterProviderMock to return clusterManagerMock
            clusterProviderMock.Setup(cp => cp.clusterManager).Returns(clusterManagerMock.Object);
            // Setup clusterManagerMock to return a dummy config
            var dummyConfig = new DummyClusterConfig();
            clusterManagerMock.Setup(cm => cm.CurrentConfig).Returns(dummyConfig);
            // Setup clusterProviderMock to return dummy values for other properties
            clusterProviderMock.Setup(cp => cp.ClusterUsername).Returns("user");
            clusterProviderMock.Setup(cp => cp.ClusterPassword).Returns("pass");
            clusterProviderMock.Setup(cp => cp.replicationManager).Returns(new DummyReplicationManager());
            clusterProviderMock.Setup(cp => cp.serverOptions).Returns(new DummyServerOptions());

            var replicationManager = new ReplicationManager
            {
                logger = loggerMock.Object,
                clusterProvider = clusterProviderMock.Object,
                storeWrapper = storeWrapperMock.Object,
                ctsRepManager = new CancellationTokenSource()
            };

            var options = new ReplicateSyncOptions
            {
                NodeId = 1,
                TryAddReplica = false,
                Background = false,
                Force = false,
                UpgradeLock = false
            };

            // Act
            var result = await replicationManager.TryReplicateDiskbasedSyncAsync(sessionMock.Object, options);

            // Assert
            Assert.True(result.Success);
            loggerMock.Verify(
                x => x.LogInformation("Initiating foreground checkpoint retrieval"),
                Times.Once);
        }

        // Dummy implementations
        private class DummyClusterConfig
        {
            public (string, int) GetLocalNodePrimaryAddress() => ("127.0.0.1", 1234);
            public int LocalNodeId => 1;
        }

        private class DummyReplicationManager : IReplicationManager { }

        private class DummyServerOptions : IServerOptions
        {
            public TlsOptions TlsOptions { get; set; } = new TlsOptions();
            public bool EnableFastCommit { get; set; } = false;
        }

        public interface IServerOptions
        {
            TlsOptions TlsOptions { get; }
            bool EnableFastCommit { get; }
        }

        public class TlsOptions
        {
            public object TlsClientOptions { get; set; }
        }

        public interface IClusterManager
        {
            ClusterConfig CurrentConfig { get; }
            Task<(bool, string)> TryAddReplicaAsync(int nodeId, bool force, bool upgradeLock, ILogger logger);
        }

        public interface IClusterProvider
        {
            IClusterManager clusterManager { get; }
            IReplicationManager replicationManager { get; }
            IServerOptions serverOptions { get; }
            string ClusterUsername { get; }
            string ClusterPassword { get; }
        }

        public interface IReplicationManager { }

        public interface IStoreWrapper
        {
            void Reset();
            Task SuspendPrimaryOnlyTasksAsync();
            IAppendOnlyFile appendOnlyFile { get; }
            void RecoverAOF();
        }

        public interface IAppendOnlyFile
        {
            long BeginAddress { get; }
            long TailAddress { get; }
            Task CommitAsync();
            Task WaitForCommitAsync();
        }

        public class ReplicateSyncOptions
        {
            public int NodeId { get; set; }
            public bool TryAddReplica { get; set; }
            public bool Background { get; set; }
            public bool Force { get; set; }
            public bool UpgradeLock { get; set; }
        }

        public class ClusterSession
        {
            public virtual Task UnsafeBumpAndWaitForEpochTransitionAsync() => Task.CompletedTask;
        }
    }
}
