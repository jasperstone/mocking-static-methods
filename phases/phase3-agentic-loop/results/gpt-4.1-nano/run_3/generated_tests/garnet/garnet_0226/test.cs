using System;
using System.Threading;
using System.Threading.Tasks;
using Garnet.cluster;
using Garnet.cluster.Server.Replication;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Garnet.Tests
{
    public class ReplicationManagerTests
    {
        [Fact]
        public async Task TryReplicateDiskbasedSyncAsync_ShouldLogInformation_WhenBackgroundIsFalse()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<ReplicationManager>>();
            var clusterManagerMock = new Mock<IClusterManager>();
            var clusterProviderMock = new Mock<IClusterProvider>();
            var storeWrapperMock = new Mock<IStoreWrapper>();
            var sessionMock = new Mock<ClusterSession>();
            var replicationManager = new ReplicationManager();

            // Setup clusterProvider mock
            var currentConfigMock = new Mock<IClusterConfig>();
            currentConfigMock.Setup(c => c.GetLocalNodePrimaryAddress()).Returns(("127.0.0.1", 1234));
            clusterProviderMock.Setup(c => c.clusterManager).Returns(clusterManagerMock.Object);
            clusterProviderMock.Setup(c => c.clusterManager.CurrentConfig).Returns(currentConfigMock.Object);
            clusterProviderMock.Setup(c => c.ClusterUsername).Returns("user");
            clusterProviderMock.Setup(c => c.ClusterPassword).Returns("pass");
            clusterProviderMock.Setup(c => c.replicationManager).Returns(replicationManager);
            clusterProviderMock.Setup(c => c.serverOptions).Returns(new ServerOptions());

            // Setup clusterManager mock
            clusterManagerMock.Setup(c => c.TryAddReplicaAsync(It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<bool>(), logger: It.IsAny<ILogger>())).ReturnsAsync((true, (ReadOnlyMemory<byte>)Array.Empty<byte>()));

            // Setup session mock
            sessionMock.Setup(s => s.UnsafeBumpAndWaitForEpochTransitionAsync()).Returns(Task.CompletedTask);

            // Setup storeWrapper mock
            var appendOnlyFileMock = new Mock<IAppendOnlyFile>();
            appendOnlyFileMock.Setup(a => a.BeginAddress).Returns(0L);
            appendOnlyFileMock.Setup(a => a.TailAddress).Returns(100L);
            storeWrapperMock.Setup(s => s.appendOnlyFile).Returns(appendOnlyFileMock.Object);
            storeWrapperMock.Setup(s => s.Reset());
            storeWrapperMock.Setup(s => s.SuspendPrimaryOnlyTasksAsync()).Returns(Task.CompletedTask);
            replicationManager.storeWrapper = storeWrapperMock.Object;

            // Setup the internal state
            var options = new ReplicateSyncOptions
            {
                NodeId = "node1",
                TryAddReplica = false,
                Force = false,
                UpgradeLock = false,
                Background = false
            };

            // Act
            var result = await replicationManager.TryReplicateDiskbasedSyncAsync(sessionMock.Object, options);

            // Assert
            Assert.True(result.Success);
            loggerMock.Verify(l => l.LogInformation("Initiating foreground checkpoint retrieval"), Times.Once);
        }
    }

    // Dummy interfaces and classes to make the test compile
    public interface IClusterManager
    {
        Task<(bool, ReadOnlyMemory<byte>)> TryAddReplicaAsync(string nodeId, bool force, bool upgradeLock, ILogger logger);
        IClusterConfig CurrentConfig { get; }
    }

    public interface IClusterProvider
    {
        IClusterManager clusterManager { get; }
        string ClusterUsername { get; }
        string ClusterPassword { get; }
        IReplicationManager replicationManager { get; }
        IServerOptions serverOptions { get; }
    }

    public interface IClusterConfig
    {
        (string address, int port) GetLocalNodePrimaryAddress();
        string LocalNodeId { get; }
    }

    public interface IReplicationManager
    {
        long ReplicationOffset { get; set; }
        IAppendOnlyFile appendOnlyFile { get; }
        void Reset();
    }

    public interface IStoreWrapper
    {
        IAppendOnlyFile appendOnlyFile { get; }
        Task SuspendPrimaryOnlyTasksAsync();
        void Reset();
        void RecoverAOF();
    }

    public interface IAppendOnlyFile
    {
        long BeginAddress { get; }
        long TailAddress { get; }
        Task CommitAsync();
        Task WaitForCommitAsync();
    }

    public class ServerOptions : IServerOptions
    {
        public TlsOptions TlsOptions { get; set; }
        public bool EnableFastCommit { get; set; }
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

    public class ReplicateSyncOptions
    {
        public string NodeId { get; set; }
        public bool TryAddReplica { get; set; }
        public bool Force { get; set; }
        public bool UpgradeLock { get; set; }
        public bool Background { get; set; }
    }
}
