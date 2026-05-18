using System;
using System.Threading;
using System.Threading.Tasks;
using Garnet.cluster;
using Garnet.common;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Garnet.Tests
{
    public class ReplicaFailoverSessionTests
    {
        private readonly Mock<ILogger<FailoverSession>> _loggerMock;
        private readonly Mock<ClusterProvider> _clusterProviderMock;
        private readonly Mock<ClusterManager> _clusterManagerMock;
        private readonly Mock<ReplicationManager> _replicationManagerMock;
        private readonly Mock<StoreWrapper> _storeWrapperMock;
        private readonly Mock<ClusterConfig> _configMock;
        private readonly Mock<GarnetClient> _clientMock;

        public ReplicaFailoverSessionTests()
        {
            _loggerMock = new Mock<ILogger<FailoverSession>>();
            _clusterProviderMock = new Mock<ClusterProvider>();
            _clusterManagerMock = new Mock<ClusterManager>();
            _replicationManagerMock = new Mock<ReplicationManager>();
            _storeWrapperMock = new Mock<StoreWrapper>();
            _configMock = new Mock<ClusterConfig>();
            _clientMock = new Mock<GarnetClient>();

            // Setup default behaviors
            _clusterProviderMock.Setup(cp => cp.clusterManager).Returns(_clusterManagerMock.Object);
            _clusterProviderMock.Setup(cp => cp.replicationManager).Returns(_replicationManagerMock.Object);
            _clusterProviderMock.Setup(cp => cp.storeWrapper).Returns(_storeWrapperMock.Object);
            _clusterProviderMock.Setup(cp => cp.clusterManager.CurrentConfig).Returns(_configMock.Object);
            _clusterProviderMock.Setup(cp => cp.ClusterUsername).Returns("user");
            _clusterProviderMock.Setup(cp => cp.ClusterPassword).Returns("pass");
            _clusterProviderMock.Setup(cp => cp.serverOptions).Returns(new ServerOptions());
            _clusterProviderMock.Setup(cp => cp.BumpAndWaitForEpochTransitionAsync()).ReturnsAsync(true);
            _clusterProviderMock.Setup(cp => cp.GetEndpointFromNodeId(It.IsAny<string>())).Returns("endpoint");
            _clusterProviderMock.Setup(cp => cp.clusterManager.TryTakeOverForPrimary()).Returns(true);
            _clusterProviderMock.Setup(cp => cp.replicationManager.TryUpdateForFailover());
            _clusterProviderMock.Setup(cp => cp.replicationManager.ResetReplayIterator());
            _clusterProviderMock.Setup(cp => cp.replicationManager.InitializeCheckpointStore()).Returns(true);
            _clusterProviderMock.Setup(cp => cp.BumpAndWaitForEpochTransitionAsync()).ReturnsAsync(true);
            _clusterProviderMock.Setup(cp => cp.storeWrapper.StartPrimaryTasks());

            // Setup config mock
            _configMock.Setup(c => c.LocalNodePrimaryId).Returns("primary");
            _configMock.Setup(c => c.LocalNodeId).Returns("local");
        }

        [Fact]
        public async Task BroadcastConfigAndRequestAttachAsync_Should_LogWarning_When_TryTakeOverFails()
        {
            // Arrange
            var session = new FailoverSession(
                _clusterProviderMock.Object,
                FailoverOption.DEFAULT,
                TimeSpan.FromSeconds(10),
                TimeSpan.FromSeconds(10),
                new LightEpoch(1),
                isReplicaSession: true,
                logger: _loggerMock.Object);

            // Force TryTakeOverForPrimary to return false to trigger warning
            _clusterProviderMock.Setup(cp => cp.clusterManager.TryTakeOverForPrimary()).Returns(false);

            // Setup client mock to simulate GossipAsync
            var gossipTask = Task.FromResult(new Mock<IAsyncEnumerable<object>>().Object);
            _clientMock.Setup(c => c.GossipAsync(It.IsAny<byte[]>())).ReturnsAsync(gossipTask);

            // Act
            await session.BroadcastConfigAndRequestAttachAsync("replicaId", new byte[] { 1, 2, 3 });

            // Assert
            _loggerMock.Verify(
                x => x.LogWarning(It.IsAny<string>(), It.IsAny<object[]>()),
                Times.Once);
        }
    }
}
