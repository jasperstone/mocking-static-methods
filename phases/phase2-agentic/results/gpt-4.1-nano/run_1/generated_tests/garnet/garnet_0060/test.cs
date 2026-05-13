using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using Garnet.cluster;

namespace Garnet.Tests
{
    public class ReplicaFailoverSessionTests
    {
        private readonly Mock<ILogger<ReplicaFailoverSession>> _loggerMock;
        private readonly Mock<cluster.ClusterProvider> _clusterProviderMock;
        private readonly Mock<cluster.ClusterManager> _clusterManagerMock;
        private readonly Mock<cluster.ReplicationManager> _replicationManagerMock;
        private readonly Mock<cluster.StoreWrapper> _storeWrapperMock;
        private readonly Mock<GarnetClient> _clientMock;
        private readonly ReplicaFailoverSession _session;

        public ReplicaFailoverSessionTests()
        {
            _loggerMock = new Mock<ILogger<ReplicaFailoverSession>>();
            _clusterProviderMock = new Mock<cluster.ClusterProvider>();
            _clusterManagerMock = new Mock<cluster.ClusterManager>();
            _replicationManagerMock = new Mock<cluster.ReplicationManager>();
            _storeWrapperMock = new Mock<cluster.StoreWrapper>();
            _clientMock = new Mock<GarnetClient>();

            // Setup mocks
            _clusterProviderMock.Setup(cp => cp.clusterManager).Returns(_clusterManagerMock.Object);
            _clusterProviderMock.Setup(cp => cp.replicationManager).Returns(_replicationManagerMock.Object);
            _clusterProviderMock.Setup(cp => cp.storeWrapper).Returns(_storeWrapperMock.Object);
            _clusterProviderMock.Setup(cp => cp.ClusterUsername).Returns("user");
            _clusterProviderMock.Setup(cp => cp.ClusterPassword).Returns("pass");
            _clusterProviderMock.Setup(cp => cp.serverOptions).Returns(new ServerOptions());
            _clusterProviderMock.Setup(cp => cp.CurrentConfig).Returns(new ClusterConfig());
            _clusterProviderMock.Setup(cp => cp.BumpAndWaitForEpochTransitionAsync()).ReturnsAsync(true);

            // Setup oldConfig with necessary properties
            var oldConfig = new Mock<ClusterConfig>();
            oldConfig.Setup(c => c.LocalNodeId).Returns("node1");
            oldConfig.Setup(c => c.LocalNodePrimaryId).Returns("primary1");
            oldConfig.Setup(c => c.GetEndpointFromNodeId(It.IsAny<string>())).Returns("endpoint");
            _clusterProviderMock.Setup(cp => cp.oldConfig).Returns(oldConfig.Object);

            // Setup clusterManager.CurrentConfig
            var currentConfig = new Mock<ClusterConfig>();
            currentConfig.Setup(c => c.GetReplicaIds(It.IsAny<string>())).Returns(new List<string> { "replica1", "replica2" });
            currentConfig.Setup(c => c.ToByteArray()).Returns(new byte[] { 1, 2, 3 });
            _clusterManagerMock.Setup(cm => cm.CurrentConfig).Returns(currentConfig.Object);

            // Setup clusterManager.TryTakeOverForPrimary
            _clusterManagerMock.Setup(cm => cm.TryTakeOverForPrimary()).Returns(true);

            // Setup replicationManager methods
            _replicationManagerMock.Setup(rm => rm.BeginRecovery(It.IsAny<RecoveryStatus>(), false)).Returns(true);
            _replicationManagerMock.Setup(rm => rm.TryUpdateForFailover());
            _replicationManagerMock.Setup(rm => rm.ResetReplayIterator());
            _replicationManagerMock.Setup(rm => rm.InitializeCheckpointStore()).Returns(true);
            _replicationManagerMock.Setup(rm => rm.EndRecovery(RecoveryStatus.NoRecovery, false));

            // Setup storeWrapper
            _storeWrapperMock.Setup(sw => sw.StartPrimaryTasks());

            // Setup GarnetClient mock
            _clientMock.Setup(c => c.GossipAsync(It.IsAny<byte[]>())).ReturnsAsync(new Mock<IAsyncEnumerable<byte[]>>().Object);
            _clientMock.Setup(c => c.FailStopWritesAsync(It.IsAny<byte[]>())).ReturnsAsync(0L);

            // Instantiate session
            _session = new ReplicaFailoverSession(_clusterProviderMock.Object, _loggerMock.Object);
        }

        [Fact]
        public async Task LogWarning_Called_When_Gossip_From_Unknown_Node()
        {
            // Arrange
            var config = new ClusterConfig();
            var otherConfig = new ClusterConfig();
            otherConfig.LocalNodeId = "unknownNode";

            // Setup clusterManager.CurrentConfig to return config
            _clusterManagerMock.Setup(c => c.CurrentConfig).Returns(config);

            // Setup oldConfig.LocalNodePrimaryId
            var oldConfig = new Mock<ClusterConfig>();
            oldConfig.Setup(c => c.LocalNodePrimaryId).Returns("primary1");
            _clusterProviderMock.Setup(cp => cp.oldConfig).Returns(oldConfig.Object);

            // Setup clusterManager.TryTakeOverForPrimary to return true
            _clusterManagerMock.Setup(c => c.TryTakeOverForPrimary()).Returns(true);

            // Act
            await _session.BroadcastConfigAndRequestAttachAsync("unknownNode", new byte[] { 1, 2, 3 });

            // Assert
            _loggerMock.Verify(
                x => x.LogWarning(It.Is<string>(s => s.Contains("Received gossip from unknown node")), It.IsAny<object[]>()),
                Times.Once);
        }
    }
}
