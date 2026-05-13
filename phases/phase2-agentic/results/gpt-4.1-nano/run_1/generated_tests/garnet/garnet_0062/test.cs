using System;
using System.Collections.Generic;
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
        private readonly Mock<ILogger<ReplicaFailoverSession>> _loggerMock;
        private readonly Mock<ClusterProvider> _clusterProviderMock;
        private readonly Mock<ClusterManager> _clusterManagerMock;
        private readonly Mock<ReplicationManager> _replicationManagerMock;
        private readonly Mock<StoreWrapper> _storeWrapperMock;
        private readonly Mock<FailoverStatus> _failoverStatusMock;
        private readonly Mock<GarnetClient> _garnetClientMock;
        private readonly ReplicaFailoverSession _session;

        public ReplicaFailoverSessionTests()
        {
            _loggerMock = new Mock<ILogger<ReplicaFailoverSession>>();
            _clusterProviderMock = new Mock<ClusterProvider>();
            _clusterManagerMock = new Mock<ClusterManager>();
            _replicationManagerMock = new Mock<ReplicationManager>();
            _storeWrapperMock = new Mock<StoreWrapper>();
            _failoverStatusMock = new Mock<FailoverStatus>();
            _garnetClientMock = new Mock<GarnetClient>();

            // Setup mocks
            _clusterProviderMock.Setup(cp => cp.clusterManager).Returns(_clusterManagerMock.Object);
            _clusterProviderMock.Setup(cp => cp.replicationManager).Returns(_replicationManagerMock.Object);
            _clusterProviderMock.Setup(cp => cp.storeWrapper).Returns(_storeWrapperMock.Object);
            _clusterProviderMock.Setup(cp => cp.ClusterUsername).Returns("user");
            _clusterProviderMock.Setup(cp => cp.ClusterPassword).Returns("pass");
            _clusterProviderMock.Setup(cp => cp.serverOptions).Returns(new ServerOptions());
            _clusterProviderMock.Setup(cp => cp.BumpAndWaitForEpochTransitionAsync()).ReturnsAsync(true);
            _clusterProviderMock.Setup(cp => cp.clusterManager.CurrentConfig).Returns(new ClusterConfig());
            _clusterProviderMock.Setup(cp => cp.GetConnectionAsync(It.IsAny<string>())).ReturnsAsync(_garnetClientMock.Object);
            _clusterProviderMock.Setup(cp => cp.clusterManager.TryTakeOverForPrimary()).Returns(true);
            _clusterProviderMock.Setup(cp => cp.replicationManager.BeginRecovery(It.IsAny<RecoveryStatus>(), false)).Returns(true);
            _clusterProviderMock.Setup(cp => cp.replicationManager.TryUpdateForFailover());
            _clusterProviderMock.Setup(cp => cp.replicationManager.ResetReplayIterator());
            _clusterProviderMock.Setup(cp => cp.replicationManager.InitializeCheckpointStore()).Returns(true);
            _clusterProviderMock.Setup(cp => cp.storeWrapper.StartPrimaryTasks());

            _session = new ReplicaFailoverSession(_loggerMock.Object, _clusterProviderMock.Object)
            {
                oldConfig = new OldConfig { LocalNodePrimaryId = "primary", LocalNodeId = "node1" },
                status = FailoverStatus.WAITING_FOR_SYNC,
                cts = new CancellationTokenSource(),
                failoverTimeout = TimeSpan.FromSeconds(10),
                epoch = 1
            };
        }

        [Fact]
        public async Task BroadcastConfigAndRequestAttachAsync_LogsError_WhenClientIsNull()
        {
            // Arrange
            var replicaId = "replica1";
            var configBytes = new byte[] { 1, 2, 3 };
            _clusterProviderMock.Setup(cp => cp.clusterManager.CurrentConfig).Returns(new ClusterConfig());
            _clusterProviderMock.Setup(cp => cp.GetConnectionAsync(replicaId)).ReturnsAsync((GarnetClient)null);

            // Act
            await _session.BroadcastConfigAndRequestAttachAsync(replicaId, configBytes);

            // Assert
            _loggerMock.Verify(
                x => x.LogError(It.IsAny<string>(), It.IsAny<object[]>()),
                Times.Once);
        }

        [Fact]
        public async Task BroadcastConfigAndRequestAttachAsync_LogsWarning_WhenTryTakeOverForPrimaryFails()
        {
            // Arrange
            var replicaId = "replica2";
            var configBytes = new byte[] { 4, 5, 6 };
            _clusterProviderMock.Setup(cp => cp.clusterManager.TryTakeOverForPrimary()).Returns(false);
            _clusterProviderMock.Setup(cp => cp.clusterManager.CurrentConfig).Returns(new ClusterConfig());

            // Act
            var result = await _session.BroadcastConfigAndRequestAttachAsync(replicaId, configBytes);

            // Assert
            Assert.Null(result);
            _loggerMock.Verify(
                x => x.LogWarning(It.IsAny<string>(), It.IsAny<object[]>()),
                Times.Once);
        }

        [Fact]
        public async Task BroadcastConfigAndRequestAttachAsync_LogsError_WhenGossipAsyncFails()
        {
            // Arrange
            var replicaId = "replica3";
            var configBytes = new byte[] { 7, 8, 9 };
            var mockClient = new Mock<GarnetClient>();
            mockClient.Setup(c => c.GossipAsync(It.IsAny<byte[]>())).ReturnsAsync((Response)null);
            _clusterProviderMock.Setup(cp => cp.GetConnectionAsync(replicaId)).ReturnsAsync(mockClient.Object);
            _clusterProviderMock.Setup(cp => cp.clusterManager.CurrentConfig).Returns(new ClusterConfig());

            // Act
            await _session.BroadcastConfigAndRequestAttachAsync(replicaId, configBytes);

            // Assert
            _loggerMock.Verify(
                x => x.LogError(It.IsAny<string>(), It.IsAny<object[]>()),
                Times.Once);
        }

        [Fact]
        public async Task BroadcastConfigAndRequestAttachAsync_LogsWarning_WhenGossipResponseIndicatesError()
        {
            // Arrange
            var replicaId = "replica4";
            var configBytes = new byte[] { 10, 11, 12 };
            var mockClient = new Mock<GarnetClient>();
            var mockResponse = new Response { IsError = true, ErrorMessage = "Error" };
            mockClient.Setup(c => c.GossipAsync(It.IsAny<byte[]>())).ReturnsAsync(mockResponse);
            _clusterProviderMock.Setup(cp => cp.GetConnectionAsync(replicaId)).ReturnsAsync(mockClient.Object);
            _clusterProviderMock.Setup(cp => cp.clusterManager.CurrentConfig).Returns(new ClusterConfig());

            // Act
            await _session.BroadcastConfigAndRequestAttachAsync(replicaId, configBytes);

            // Assert
            _loggerMock.Verify(
                x => x.LogWarning(It.IsAny<string>(), It.IsAny<object[]>()),
                Times.Once);
        }
    }

    // Dummy classes to satisfy references
    public class ClusterProvider
    {
        public ClusterManager clusterManager { get; set; }
        public ReplicationManager replicationManager { get; set; }
        public StoreWrapper storeWrapper { get; set; }
        public string ClusterUsername { get; set; }
        public string ClusterPassword { get; set; }
        public ServerOptions serverOptions { get; set; }
        public ClusterConfig CurrentConfig { get; set; }
        public Task<GarnetClient> GetConnectionAsync(string nodeId) => Task.FromResult(new GarnetClient());
        public Task BumpAndWaitForEpochTransitionAsync() => Task.CompletedTask;
    }

    public class ClusterManager
    {
        public ClusterConfig CurrentConfig { get; set; }
        public bool TryTakeOverForPrimary() => true;
    }

    public class ReplicationManager
    {
        public bool BeginRecovery(RecoveryStatus status, bool upgradeLock) => true;
        public void TryUpdateForFailover() { }
        public void ResetReplayIterator() { }
        public bool InitializeCheckpointStore() => true;
        public void EndRecovery(RecoveryStatus status, bool downgradeLock) { }
    }

    public class StoreWrapper
    {
        public void StartPrimaryTasks() { }
    }

    public class ClusterConfig { }

    public class Response
    {
        public bool IsError { get; set; }
        public string ErrorMessage { get; set; }
    }

    public class GarnetClient
    {
        public bool IsConnected => true;
        public Task ReconnectAsync() => Task.CompletedTask;
        public Task<FailureResponse> FailStopWritesAsync(byte[] localIdBytes) => Task.FromResult(new FailureResponse());
        public Task<Response> GossipAsync(byte[] configBytes) => Task.FromResult(new Response());
        public void Dispose() { }
    }

    public class FailureResponse { }

    public enum FailoverStatus
    {
        WAITING_FOR_SYNC,
        ISSUING_PAUSE_WRITES,
        TAKING_OVER_AS_PRIMARY
    }

    public class OldConfig
    {
        public string LocalNodePrimaryId { get; set; }
        public string LocalNodeId { get; set; }
        public string GetEndpointFromNodeId(string nodeId) => "endpoint";
    }

    public class ServerOptions
    {
        public TlsOptions TlsOptions { get; set; }
    }

    public class TlsOptions
    {
        public object TlsClientOptions { get; set; }
    }

    public static class CmdStrings
    {
        public static readonly byte[] RESP_ERR_GENERIC_CANNOT_ACQUIRE_RECOVERY_LOCK = new byte[0];
        public static readonly byte[] RESP_ERR_GENERIC_CANNOT_TAKEOVER_FROM_PRIMARY = new byte[0];
    }
}
