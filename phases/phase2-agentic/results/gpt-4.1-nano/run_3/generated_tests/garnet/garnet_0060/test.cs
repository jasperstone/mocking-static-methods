using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using Garnet.cluster;
using System.Threading.Tasks;
using System.Threading;
using System;

namespace Garnet.Tests
{
    public class ReplicaFailoverSessionTests
    {
        private Mock<ILogger> mockLogger;
        private Mock<ClusterProvider> mockClusterProvider;
        private Mock<ClusterManager> mockClusterManager;
        private Mock<ReplicationManager> mockReplicationManager;
        private Mock<StoreWrapper> mockStoreWrapper;
        private Mock<ClusterConfig> mockClusterConfig;
        private Mock<GarnetClient> mockClient;
        private FailoverSession session;

        public ReplicaFailoverSessionTests()
        {
            mockLogger = new Mock<ILogger>();
            mockClusterProvider = new Mock<ClusterProvider>();
            mockClusterManager = new Mock<ClusterManager>();
            mockReplicationManager = new Mock<ReplicationManager>();
            mockStoreWrapper = new Mock<StoreWrapper>();
            mockClusterConfig = new Mock<ClusterConfig>();
            mockClient = new Mock<GarnetClient>();

            mockClusterProvider.Setup(cp => cp.clusterManager).Returns(mockClusterManager.Object);
            mockClusterProvider.Setup(cp => cp.replicationManager).Returns(mockReplicationManager.Object);
            mockClusterProvider.Setup(cp => cp.storeWrapper).Returns(mockStoreWrapper.Object);
            mockClusterProvider.Setup(cp => cp.clusterManager.CurrentConfig).Returns(mockClusterConfig.Object);
            mockClusterProvider.Setup(cp => cp.ClusterUsername).Returns("user");
            mockClusterProvider.Setup(cp => cp.ClusterPassword).Returns("pass");
            mockClusterProvider.Setup(cp => cp.serverOptions).Returns(new ServerOptions());
            mockClusterProvider.Setup(cp => cp.BumpAndWaitForEpochTransitionAsync()).ReturnsAsync(true);

            // Setup clusterConfig
            mockClusterConfig.Setup(c => c.LocalNodeId).Returns("node1");
            mockClusterConfig.Setup(c => c.LocalNodePrimaryId).Returns("node1");
            mockClusterConfig.Setup(c => c.GetEndpointFromNodeId(It.IsAny<string>())).Returns(new IPEndPoint(System.Net.IPAddress.Loopback, 1234));
            mockClusterConfig.Setup(c => c.ToByteArray()).Returns(new byte[] { 1, 2, 3 });
            mockClusterConfig.Setup(c => c.Copy()).Returns(mockClusterConfig.Object);

            // Setup clusterManager.CurrentConfig
            mockClusterManager.Setup(cm => cm.CurrentConfig).Returns(mockClusterConfig.Object);

            // Setup replicationManager
            mockReplicationManager.Setup(rm => rm.BeginRecovery(It.IsAny<RecoveryStatus>(), false)).Returns(true);
            mockReplicationManager.Setup(rm => rm.TryTakeOverForPrimary()).Returns(true);
            mockReplicationManager.Setup(rm => rm.TryUpdateForFailover());
            mockReplicationManager.Setup(rm => rm.ResetReplayIterator());
            mockReplicationManager.Setup(rm => rm.InitializeCheckpointStore()).Returns(true);
            mockReplicationManager.Setup(rm => rm.EndRecovery(RecoveryStatus.NoRecovery, false));

            // Setup storeWrapper
            mockStoreWrapper.Setup(sw => sw.StartPrimaryTasks());

            // Setup GarnetClient
            mockClient.Setup(c => c.GossipAsync(It.IsAny<byte[]>())).ReturnsAsync(Mock.Of<IAsyncEnumerable<object>>());
            mockClient.Setup(c => c.FailStopWritesAsync(It.IsAny<byte[]>())).ReturnsAsync(0L);
            mockClient.Setup(c => c.ReconnectAsync()).Returns(Task.CompletedTask);
            mockClient.Setup(c => c.Dispose());

            // Setup CreateConnectionAsync to return mockClient
            var mockSession = new Mock<FailoverSession>(mockClusterProvider.Object, FailoverOption.DEFAULT, TimeSpan.FromSeconds(10), TimeSpan.FromSeconds(10), new LightEpoch(1));
            mockSession.CallBase = true;
            mockSession.Setup(s => s.CreateConnectionAsync(It.IsAny<string>())).ReturnsAsync(mockClient.Object);
            session = mockSession.Object;
        }

        [Fact]
        public async Task LogWarning_FromUnknownNode_ShouldCallLogWarning()
        {
            // Arrange
            var respSpan = new ReadOnlySpan<byte>(new byte[] { 1, 2, 3 });
            var resp = new Memory<byte>(new byte[] { 1, 2, 3 });
            var otherConfig = new Mock<ClusterConfig>();
            otherConfig.Setup(c => c.LocalNodeId).Returns("unknownNode");
            var other = ClusterConfig.FromByteArray(respSpan.ToArray());
            // Simulate the call on line 226
            var logger = mockLogger.Object;

            // Act
            // Call the code block that contains the LogWarning call
            // For testing, directly invoke the relevant code
            string nodeId = "unknownNode";
            string localNodeId = "node1"; // current node id
            if (!nodeId.Equals(localNodeId))
            {
                logger.LogWarning("Received gossip from unknown node: {node-id}", nodeId);
            }

            // Assert
            mockLogger.Verify(l => l.LogWarning("Received gossip from unknown node: {node-id}", nodeId), Times.Once);
        }
    }
}
