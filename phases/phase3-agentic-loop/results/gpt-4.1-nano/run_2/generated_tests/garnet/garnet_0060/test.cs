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
        private readonly Mock<ClusterConfig> _clusterConfigMock;
        private readonly Mock<GarnetClient> _garnetClientMock;
        private readonly FailoverSession _session;

        public ReplicaFailoverSessionTests()
        {
            _loggerMock = new Mock<ILogger<FailoverSession>>();
            _clusterProviderMock = new Mock<ClusterProvider>();
            _clusterManagerMock = new Mock<ClusterManager>();
            _replicationManagerMock = new Mock<ReplicationManager>();
            _storeWrapperMock = new Mock<StoreWrapper>();
            _clusterConfigMock = new Mock<ClusterConfig>();
            _garnetClientMock = new Mock<GarnetClient>();

            // Setup clusterProvider mock to return mocks for properties
            _clusterProviderMock.Setup(cp => cp.clusterManager).Returns(_clusterManagerMock.Object);
            _clusterProviderMock.Setup(cp => cp.replicationManager).Returns(_replicationManagerMock.Object);
            _clusterProviderMock.Setup(cp => cp.storeWrapper).Returns(_storeWrapperMock.Object);
            _clusterProviderMock.Setup(cp => cp.clusterManager.CurrentConfig).Returns(_clusterConfigMock.Object);
            _clusterProviderMock.Setup(cp => cp.ClusterUsername).Returns("user");
            _clusterProviderMock.Setup(cp => cp.ClusterPassword).Returns("pass");
            _clusterProviderMock.Setup(cp => cp.serverOptions).Returns(new ServerOptions());
            _clusterProviderMock.Setup(cp => cp.BumpAndWaitForEpochTransitionAsync()).ReturnsAsync(true);

            // Setup clusterManager mock
            _clusterManagerMock.Setup(cm => cm.TryTakeOverForPrimary()).Returns(true);
            _clusterManagerMock.Setup(cm => cm.CurrentConfig).Returns(_clusterConfigMock.Object);

            // Setup replicationManager mock
            _replicationManagerMock.Setup(rm => rm.BeginRecovery(It.IsAny<RecoveryStatus>(), false)).Returns(true);
            _replicationManagerMock.Setup(rm => rm.TryUpdateForFailover());
            _replicationManagerMock.Setup(rm => rm.ResetReplayIterator());
            _replicationManagerMock.Setup(rm => rm.InitializeCheckpointStore()).Returns(true);
            _replicationManagerMock.Setup(rm => rm.EndRecovery(RecoveryStatus.NoRecovery, false));

            // Setup storeWrapper mock
            _storeWrapperMock.Setup(sw => sw.StartPrimaryTasks());

            // Setup clusterConfig mock
            _clusterConfigMock.Setup(c => c.LocalNodeId).Returns("node1");
            _clusterConfigMock.Setup(c => c.LocalNodePrimaryId).Returns("primary1");
            _clusterConfigMock.Setup(c => c.ToByteArray()).Returns(new byte[] { 1, 2, 3 });
            _clusterConfigMock.Setup(c => c.LocalNodeIp).Returns("127.0.0.1");
            _clusterConfigMock.Setup(c => c.LocalNodePort).Returns(1234);

            // Setup GarnetClient mock
            _garnetClientMock.Setup(c => c.GossipAsync(It.IsAny<byte[]>())).ReturnsAsync(_garnetClientMock.Object);
            _garnetClientMock.Setup(c => c.WaitAsync(It.IsAny<TimeSpan>(), It.IsAny<CancellationToken>())).ReturnsAsync(_garnetClientMock.Object);
            _garnetClientMock.Setup(c => c.FailStopWritesAsync(It.IsAny<byte[]>())).ReturnsAsync(0L);

            // Instantiate FailoverSession with mocks
            _session = new FailoverSession(
                _clusterProviderMock.Object,
                FailoverOption.DEFAULT,
                TimeSpan.FromSeconds(10),
                TimeSpan.FromSeconds(10),
                new LightEpoch(1),
                isReplicaSession: true,
                logger: _loggerMock.Object);
        }

        [Fact]
        public async Task LogWarning_IsCalled_When_GossipResponse_FromUnknownNode()
        {
            // Arrange
            var unknownNodeId = "unknownNode";
            var configData = new byte[] { 1, 2, 3 };
            var respMock = new Mock<IGossipResponse>();
            var spanMock = new Mock<ReadOnlySpan<byte>>();
            spanMock.Setup(s => s.ToArray()).Returns(configData);
            respMock.Setup(r => r.Span).Returns(spanMock.Object);

            // Setup clusterManager.CurrentConfig to return a config with LocalNodeId
            _clusterManagerMock.Setup(cm => cm.CurrentConfig).Returns(_clusterConfigMock.Object);
            _clusterConfigMock.Setup(c => c.LocalNodeId).Returns("node1");

            // Setup GossipAsync to return the mock response
            var clientMock = new Mock<GarnetClient>();
            clientMock.Setup(c => c.GossipAsync(It.IsAny<byte[]>())).ReturnsAsync(clientMock.Object);
            // Replace the primaryClient with our mock
            typeof(FailoverSession).GetField("primaryClient", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).SetValue(_session, clientMock.Object);

            // Act
            await _session.BroadcastConfigAndRequestAttachAsync(unknownNodeId, configData);

            // Assert
            _loggerMock.Verify(
                x => x.LogWarning(It.IsAny<string>(), It.IsAny<object[]>()),
                Times.Once);
        }
    }
}
