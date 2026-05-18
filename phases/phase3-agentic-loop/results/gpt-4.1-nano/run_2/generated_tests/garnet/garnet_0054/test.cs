using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Garnet.cluster;
using Garnet.client;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Garnet.Tests
{
    public class ReplicaFailoverSessionTests
    {
        private readonly Mock<ILogger<FailoverSession>> _loggerMock;
        private readonly Mock<clusterProvider> _clusterProviderMock;
        private readonly Mock<clusterManager> _clusterManagerMock;
        private readonly Mock<ReplicationManager> _replicationManagerMock;
        private readonly Mock<storeWrapper> _storeWrapperMock;
        private readonly FailoverSession _session;

        public ReplicaFailoverSessionTests()
        {
            _loggerMock = new Mock<ILogger<FailoverSession>>();
            _clusterProviderMock = new Mock<clusterProvider>();
            _clusterManagerMock = new Mock<clusterManager>();
            _replicationManagerMock = new Mock<ReplicationManager>();
            _storeWrapperMock = new Mock<storeWrapper>();

            // Setup mocks
            _clusterProviderMock.Setup(cp => cp.clusterManager).Returns(_clusterManagerMock.Object);
            _clusterProviderMock.Setup(cp => cp.replicationManager).Returns(_replicationManagerMock.Object);
            _clusterProviderMock.Setup(cp => cp.storeWrapper).Returns(_storeWrapperMock.Object);
            _clusterProviderMock.Setup(cp => cp.CurrentConfig).Returns(new { LocalNodePrimaryId = "node1", LocalNodeId = "local1" });
            _clusterProviderMock.Setup(cp => cp.ClusterUsername).Returns("user");
            _clusterProviderMock.Setup(cp => cp.ClusterPassword).Returns("pass");
            _clusterProviderMock.Setup(cp => cp.serverOptions).Returns(new { TlsOptions = (object)null });
            _clusterProviderMock.Setup(cp => cp.GetEndpointFromNodeId(It.IsAny<string>())).Returns("endpoint");

            _session = new FailoverSession(_loggerMock.Object, _clusterProviderMock.Object);
        }

        [Fact]
        public async Task PauseWritesAndWaitForSyncAsync_Should_LogError_When_ClientIsNull()
        {
            // Arrange
            _session.oldConfig = new { LocalNodePrimaryId = "node1", LocalNodeId = "local1" };
            _session.cts = new CancellationTokenSource();
            _session.failoverTimeout = TimeSpan.FromSeconds(1);
            _session.status = FailoverStatus.NONE;
            _session.clusterProvider = _clusterProviderMock.Object;

            // Override GetConnectionAsync to return null
            _session.GetConnectionAsync = (nodeId) => Task.FromResult<GarnetClient>(null);

            // Act
            var result = await _session.PauseWritesAndWaitForSyncAsync();

            // Assert
            Assert.False(result);
            _loggerMock.Verify(
                x => x.LogError(It.IsAny<string>(), It.IsAny<object[]>()),
                Times.Once);
        }

        [Fact]
        public async Task PauseWritesAndWaitForSyncAsync_Should_ReturnTrue_When_ClientSucceedsAndOffsetCatchesUp()
        {
            // Arrange
            var mockClient = new Mock<GarnetClient>();
            mockClient.Setup(c => c.FailStopWritesAsync(It.IsAny<byte[]>())).ReturnsAsync(100);
            _session.oldConfig = new { LocalNodePrimaryId = "node1", LocalNodeId = "local1" };
            _session.cts = new CancellationTokenSource();
            _session.failoverTimeout = TimeSpan.FromSeconds(1);
            _session.status = FailoverStatus.NONE;
            _session.clusterProvider = _clusterProviderMock.Object;

            _session.GetConnectionAsync = (nodeId) => Task.FromResult(mockClient.Object);

            // Setup replication offset to be less than primary offset
            _session.clusterProvider.replicationManager = new Mock<ReplicationManager>().Object;
            _session.clusterProvider.replicationManager.ReplicationOffset = 50;

            // Act
            var result = await _session.PauseWritesAndWaitForSyncAsync();

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task PauseWritesAndWaitForSyncAsync_Should_ReturnFalse_When_TimeoutOccurs()
        {
            // Arrange
            var mockClient = new Mock<GarnetClient>();
            mockClient.Setup(c => c.FailStopWritesAsync(It.IsAny<byte[]>())).ReturnsAsync(100);
            _session.oldConfig = new { LocalNodePrimaryId = "node1", LocalNodeId = "local1" };
            _session.cts = new CancellationTokenSource();
            _session.failoverTimeout = TimeSpan.FromMilliseconds(10);
            _session.status = FailoverStatus.NONE;
            _session.clusterProvider = _clusterProviderMock.Object;

            _session.GetConnectionAsync = (nodeId) => Task.FromResult(mockClient.Object);

            // Setup replication offset to be less than primary offset
            _session.clusterProvider.replicationManager = new Mock<ReplicationManager>().Object;
            _session.clusterProvider.replicationManager.ReplicationOffset = 0;

            // Act
            var result = await _session.PauseWritesAndWaitForSyncAsync();

            // Assert
            Assert.False(result);
            _loggerMock.Verify(
                x => x.LogError("AwaitReplicationSync timed out failoverStart"),
                Times.Once);
        }
    }
}
