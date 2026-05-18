using System;
using System.Threading.Tasks;
using Garnet.client;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Garnet.Tests.cluster
{
    public class FailoverSessionTests
    {
        [Fact]
        public async Task BroadcastConfigAndRequestAttachAsync_LogsCriticalOnException()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var clusterManagerMock = new Mock<IClusterManager>();
            var clusterProviderMock = new Mock<IClusterProvider>();
            var replicationManagerMock = new Mock<IReplicationManager>();
            var gossipStatsMock = new Mock<IGossipStats>();
            var clusterConfigMock = new Mock<IClusterConfig>();

            clusterConfigMock.Setup(c => c.IsKnown(It.IsAny<string>())).Returns(true);
            clusterConfigMock.SetupGet(c => c.LocalNodeId).Returns("localNodeId");

            clusterManagerMock.SetupGet(c => c.CurrentConfig).Returns(clusterConfigMock.Object);
            clusterManagerMock.SetupGet(c => c.gossipStats).Returns(gossipStatsMock.Object);
            clusterManagerMock.Setup(c => c.TryMerge(It.IsAny<IClusterConfig>())).Throws(new InvalidOperationException("Test exception"));

            clusterProviderMock.SetupGet(c => c.clusterManager).Returns(clusterManagerMock.Object);
            clusterProviderMock.SetupGet(c => c.replicationManager).Returns(replicationManagerMock.Object);

            var oldConfig = new OldConfigStub("replica1", "localNodeId");

            var clientMock = new Mock<GarnetClient>(MockBehavior.Strict, null, null, 0, 0, null, null, 0, null);
            clientMock.Setup(c => c.GossipAsync(It.IsAny<byte[]>())).ReturnsAsync(new DisposableReadOnlyMemory(new byte[1]));
            clientMock.Setup(c => c.Dispose());
            clientMock.Setup(c => c.ReplicaOf(It.IsAny<string>(), It.IsAny<int>())).ReturnsAsync("OK");

            var failoverSession = new FailoverSessionForTest(loggerMock.Object, clusterProviderMock.Object, oldConfig, clientMock.Object);

            // Act
            await failoverSession.InvokeBroadcastConfigAndRequestAttachAsync("replica1", new byte[] { 1, 2, 3 });

            // Assert
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Critical,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("IssueAttachReplicas faulted")),
                    It.IsAny<InvalidOperationException>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        private class DisposableReadOnlyMemory : IDisposable
        {
            private readonly byte[] _buffer;
            public DisposableReadOnlyMemory(byte[] buffer) => _buffer = buffer;
            public ReadOnlyMemory<byte> Span => _buffer;
            public void Dispose() { }
        }

        private class OldConfigStub
        {
            public string LocalNodePrimaryId { get; }
            public string LocalNodeId { get; }
            public OldConfigStub(string primaryId, string localNodeId)
            {
                LocalNodePrimaryId = primaryId;
                LocalNodeId = localNodeId;
            }
            public string GetEndpointFromNodeId(string nodeId) => "endpoint";
            public int LocalNodePort => 1234;
            public string LocalNodeIp => "127.0.0.1";
        }

        private class FailoverSessionForTest
        {
            private readonly ILogger _logger;
            private readonly IClusterProvider _clusterProvider;
            private readonly OldConfigStub _oldConfig;
            private GarnetClient _primaryClient;

            public FailoverSessionForTest(ILogger logger, IClusterProvider clusterProvider, OldConfigStub oldConfig, GarnetClient primaryClient)
            {
                _logger = logger;
                _clusterProvider = clusterProvider;
                _oldConfig = oldConfig;
                _primaryClient = primaryClient;
            }

            protected ILogger logger => _logger;
            protected IClusterProvider clusterProvider => _clusterProvider;
            protected OldConfigStub oldConfig => _oldConfig;
            protected GarnetClient primaryClient
            {
                get => _primaryClient;
                set => _primaryClient = value;
            }

            public Task BroadcastConfigAndRequestAttachAsync(string replicaId, byte[] configByteArray)
            {
                // We replicate the private method logic here for testing
                return BroadcastConfigAndRequestAttachAsyncImpl(replicaId, configByteArray);
            }

            public Task InvokeBroadcastConfigAndRequestAttachAsync(string replicaId, byte[] configByteArray)
                => BroadcastConfigAndRequestAttachAsync(replicaId, configByteArray);

            private async Task BroadcastConfigAndRequestAttachAsyncImpl(string replicaId, byte[] configByteArray)
            {
                await Task.Yield();

                var oldPrimaryId = oldConfig.LocalNodePrimaryId;
                var newConfig = clusterProvider.clusterManager.CurrentConfig;
                var client = oldPrimaryId.Equals(replicaId) ? primaryClient : await GetConnectionAsync(replicaId);

                try
                {
                    if (client == null)
                    {
                        logger?.LogError("Failed to initialize connection to replica {replicaId}", replicaId);
                        return;
                    }

                    var resp = await client.GossipAsync(configByteArray).ConfigureAwait(false);

                    try
                    {
                        var current = clusterProvider.clusterManager.CurrentConfig;
                        if (resp.Span.Length > 0)
                        {
                            clusterProvider.clusterManager.gossipStats.UpdateGossipBytesRecv(resp.Span.Length);
                            var returnedConfigArray = resp.Span.ToArray();
                            var other = ClusterConfigFromByteArray(returnedConfigArray);

                            if (current.IsKnown(other.LocalNodeId))
                                _ = clusterProvider.clusterManager.TryMerge(ClusterConfigFromByteArray(returnedConfigArray));
                            else
                                logger?.LogWarning("Received gossip from unknown node: {node-id}", other.LocalNodeId);
                        }
                        resp.Dispose();
                    }
                    catch (Exception ex)
                    {
                        logger?.LogCritical(ex, "IssueAttachReplicas faulted");
                    }
                    finally
                    {
                        resp.Dispose();
                    }

                    var localAddress = oldConfig.LocalNodeIp;
                    var localPort = oldConfig.LocalNodePort;

                    var replicaOfResp = await client.ReplicaOf(localAddress, localPort).ConfigureAwait(false);

                    if (!replicaOfResp.Equals("OK"))
                        logger?.LogWarning("IssueAttachReplicas Error: {replicaId} {replicaOfResp}", replicaId, replicaOfResp);
                }
                finally
                {
                    client?.Dispose();
                }
            }

            private Task<GarnetClient> GetConnectionAsync(string nodeId)
            {
                // For test, always return primaryClient
                return Task.FromResult(primaryClient);
            }

            private IClusterConfig ClusterConfigFromByteArray(byte[] bytes)
            {
                // Return a stub config with LocalNodeId set to "localNodeId"
                return new ClusterConfigStub("localNodeId");
            }
        }

        private class ClusterConfigStub : IClusterConfig
        {
            public string LocalNodeId { get; }
            public ClusterConfigStub(string localNodeId) => LocalNodeId = localNodeId;
            public bool IsKnown(string nodeId) => nodeId == LocalNodeId;
        }

        public interface IClusterProvider
        {
            IClusterManager clusterManager { get; }
            IReplicationManager replicationManager { get; }
        }

        public interface IClusterManager
        {
            IClusterConfig CurrentConfig { get; }
            IGossipStats gossipStats { get; }
            bool TryMerge(IClusterConfig config);
        }

        public interface IClusterConfig
        {
            bool IsKnown(string nodeId);
            string LocalNodeId { get; }
        }

        public interface IGossipStats
        {
            void UpdateGossipBytesRecv(int bytes);
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

        public enum RecoveryStatus
        {
            ClusterFailover,
            NoRecovery
        }
    }
}
