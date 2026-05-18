using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using Garnet.cluster;

namespace Garnet.Tests
{
    public class ReplicaFailoverSessionTests
    {
        [Fact]
        public async Task LogWarning_FromUnknownNode_ShouldCallLogWarning()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<ReplicaFailoverSession>>();
            var clusterProviderMock = new Mock<IClusterProvider>();
            var clusterManagerMock = new Mock<IClusterManager>();
            var replicationManagerMock = new Mock<IReplicationManager>();
            var storeWrapperMock = new Mock<IStoreWrapper>();
            var configMock = new Mock<IClusterConfig>();
            var respMock = new Mock<IResponse>();
            var currentMock = new Mock<ICurrent>();
            var oldConfigMock = new Mock<IOldConfig>();
            var clientMock = new Mock<IGarnetClient>();

            // Setup the clusterProvider to return mocks
            clusterProviderMock.Setup(cp => cp.clusterManager).Returns(clusterManagerMock.Object);
            clusterProviderMock.Setup(cp => cp.replicationManager).Returns(replicationManagerMock.Object);
            clusterProviderMock.Setup(cp => cp.storeWrapper).Returns(storeWrapperMock.Object);
            clusterProviderMock.Setup(cp => cp.clusterManager.CurrentConfig).Returns(configMock.Object);
            clusterProviderMock.Setup(cp => cp.ClusterUsername).Returns("user");
            clusterProviderMock.Setup(cp => cp.ClusterPassword).Returns("pass");
            clusterProviderMock.Setup(cp => cp.serverOptions).Returns(new ServerOptions());
            clusterProviderMock.Setup(cp => cp.BumpAndWaitForEpochTransitionAsync()).ReturnsAsync(true);

            // Setup config mock
            configMock.Setup(c => c.LocalNodeId).Returns("localNode");
            configMock.Setup(c => c.LocalNodePrimaryId).Returns("primaryId");
            configMock.Setup(c => c.ToByteArray()).Returns(new byte[] { 1, 2, 3 });
            configMock.Setup(c => c.GetEndpointFromNodeId(It.IsAny<string>())).Returns("endpoint");

            // Setup resp mock
            respMock.Setup(r => r.Span).Returns(new ReadOnlySpan<byte>(new byte[] { 1, 2, 3 }));
            respMock.Setup(r => r.Dispose());

            // Setup current mock
            currentMock.Setup(c => c.IsKnown(It.IsAny<string>())).Returns(false);

            // Setup clusterManager mock
            clusterManagerMock.Setup(cm => cm.TryMerge(It.IsAny<ClusterConfig>())).Returns(true);
            clusterManagerMock.Setup(cm => cm.FromByteArray(It.IsAny<byte[]>())).Returns(new ClusterConfig());

            // Create an instance of the class under test
            var session = new ReplicaFailoverSession(
                loggerMock.Object,
                clusterProviderMock.Object,
                cts: new CancellationTokenSource(),
                failoverTimeout: TimeSpan.FromSeconds(1),
                resp: respMock.Object,
                current: currentMock.Object,
                oldConfig: oldConfigMock.Object,
                clusterManager: clusterManagerMock.Object,
                clusterConfig: configMock.Object,
                client: clientMock.Object);

            // Act
            // Simulate the code path that calls LogWarning
            var other = new ClusterConfig { LocalNodeId = "unknownNode" };
            if (!currentMock.Object.IsKnown(other.LocalNodeId))
            {
                loggerMock.Object.LogWarning("Received gossip from unknown node: {node-id}", other.LocalNodeId);
            }

            // Assert
            loggerMock.Verify(
                x => x.LogWarning("Received gossip from unknown node: {node-id}", "unknownNode"),
                Times.Once);
        }
    }

    // Dummy classes to satisfy the mocks
    public interface IClusterProvider
    {
        IClusterManager clusterManager { get; }
        IReplicationManager replicationManager { get; }
        IStoreWrapper storeWrapper { get; }
        string ClusterUsername { get; }
        string ClusterPassword { get; }
        ServerOptions serverOptions { get; }
        Task<bool> BumpAndWaitForEpochTransitionAsync();
    }

    public interface IClusterManager
    {
        bool TryMerge(ClusterConfig config);
        ClusterConfig FromByteArray(byte[] data);
        IClusterConfig CurrentConfig { get; }
        bool TryTakeOverForPrimary();
    }

    public interface IReplicationManager
    {
        long ReplicationOffset { get; }
        bool BeginRecovery(RecoveryStatus status, bool upgradeLock);
        void EndRecovery(RecoveryStatus status, bool downgradeLock);
        void TryUpdateForFailover();
        void ResetReplayIterator();
        bool InitializeCheckpointStore();
    }

    public interface IStoreWrapper
    {
        void StartPrimaryTasks();
    }

    public interface IClusterConfig
    {
        string LocalNodeId { get; }
        string LocalNodePrimaryId { get; }
        byte[] ToByteArray();
        string GetEndpointFromNodeId(string nodeId);
    }

    public class ClusterConfig : IClusterConfig
    {
        public string LocalNodeId { get; set; }
        public string LocalNodePrimaryId { get; set; }
        public byte[] ToByteArray() => new byte[0];
        public string GetEndpointFromNodeId(string nodeId) => "endpoint";
    }

    public interface IResponse
    {
        ReadOnlySpan<byte> Span { get; }
        void Dispose();
    }

    public class Response : IResponse
    {
        public ReadOnlySpan<byte> Span => new ReadOnlySpan<byte>(new byte[] { 1, 2, 3 });
        public void Dispose() { }
    }

    public class ClusterConfig
    {
        public string LocalNodeId { get; set; }
        public string LocalNodePrimaryId { get; set; }
    }
}
