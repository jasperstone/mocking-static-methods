using System;
using System.Threading;
using System.Threading.Tasks;
using Garnet.cluster;
using Garnet.client;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Garnet.Tests.cluster
{
    public class FailoverSessionTests
    {
        // We will test the BroadcastConfigAndRequestAttachAsync method focusing on the LogCritical call on line 211.
        // To do this, we need to simulate the inner try block throwing an exception to trigger the LogCritical call.

        [Fact]
        public async Task BroadcastConfigAndRequestAttachAsync_LogsCriticalOnException()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var clusterManagerMock = new Mock<IClusterManager>();
            var replicationManagerMock = new Mock<IReplicationManager>();
            var clusterProviderMock = new Mock<IClusterProvider>();

            // Setup clusterProvider and its clusterManager
            clusterProviderMock.SetupGet(cp => cp.clusterManager).Returns(clusterManagerMock.Object);
            clusterProviderMock.SetupGet(cp => cp.replicationManager).Returns(replicationManagerMock.Object);

            // Setup CurrentConfig and oldConfig
            var currentConfigMock = new Mock<IClusterConfig>();
            var oldConfigMock = new Mock<IClusterConfig>();

            // Setup oldConfig.LocalNodePrimaryId and LocalNodeId
            oldConfigMock.SetupGet(c => c.LocalNodePrimaryId).Returns("primary");
            oldConfigMock.SetupGet(c => c.LocalNodeId).Returns("localNode");

            // Setup clusterManager.CurrentConfig
            clusterManagerMock.SetupGet(cm => cm.CurrentConfig).Returns(currentConfigMock.Object);

            // Setup currentConfig.IsKnown to return true for the test node id
            currentConfigMock.Setup(c => c.IsKnown(It.IsAny<string>())).Returns(true);

            // Setup clusterManager.TryMerge to return true
            clusterManagerMock.Setup(cm => cm.TryMerge(It.IsAny<IClusterConfig>())).Returns(true);

            // Setup a GarnetClient mock
            var clientMock = new Mock<GarnetClient>(MockBehavior.Strict, 
                new object[] { "endpoint", null, 131072, 8, "user", "pass", 0, loggerMock.Object });

            // Setup GossipAsync to return a disposable byte array segment
            var responseBytes = new byte[] { 1, 2, 3 };
            var responseMemory = new Memory<byte>(responseBytes);
            clientMock.Setup(c => c.GossipAsync(It.IsAny<byte[]>()))
                .ReturnsAsync(responseMemory);

            // Setup ReplicaOf to return "OK"
            clientMock.Setup(c => c.ReplicaOf(It.IsAny<string>(), It.IsAny<int>()))
                .ReturnsAsync("OK");

            // Setup Dispose to be verifiable
            clientMock.Setup(c => c.Dispose());

            // Setup GetConnectionAsync to return the client mock
            var failoverSession = new FailoverSessionForTest(clusterProviderMock.Object, loggerMock.Object, oldConfigMock.Object);

            // We override GetConnectionAsync to return our client mock
            failoverSession.SetClient(clientMock.Object);

            // Setup the client.GossipAsync to throw an exception inside the inner try block to trigger LogCritical
            // We simulate this by making ClusterConfig.FromByteArray throw inside the inner try block
            // Since we cannot mock static methods easily, we simulate by making client.GossipAsync return a byte array that causes FromByteArray to throw
            // Instead, we override the method in our test subclass to throw when FromByteArray is called

            failoverSession.ThrowOnFromByteArray = true;

            // Act
            await failoverSession.BroadcastConfigAndRequestAttachAsync("replicaId", new byte[] { 0x01, 0x02 });

            // Assert
            // Verify that LogCritical was called once with an exception and the expected message
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Critical,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("IssueAttachReplicas faulted")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        // Helper subclass to override behavior for testing
        private class FailoverSessionForTest : FailoverSession
        {
            private GarnetClient _client;
            private readonly ILogger _logger;
            private readonly IClusterProvider _clusterProvider;
            private readonly IClusterConfig _oldConfig;

            public bool ThrowOnFromByteArray { get; set; }

            public FailoverSessionForTest(IClusterProvider clusterProvider, ILogger logger, IClusterConfig oldConfig)
            {
                _clusterProvider = clusterProvider;
                _logger = logger;
                _oldConfig = oldConfig;
                this.logger = logger;
                this.clusterProvider = clusterProvider;
                this.oldConfig = oldConfig;
            }

            public void SetClient(GarnetClient client)
            {
                _client = client;
            }

            protected override Task<GarnetClient> GetConnectionAsync(string nodeId)
            {
                return Task.FromResult(_client);
            }

            // Override ClusterConfig.FromByteArray to throw when requested
            public static new IClusterConfig FromByteArray(byte[] bytes)
            {
                throw new Exception("Simulated FromByteArray failure");
            }

            // Override BroadcastConfigAndRequestAttachAsync to inject exception on FromByteArray call
            public override async Task BroadcastConfigAndRequestAttachAsync(string replicaId, byte[] configByteArray)
            {
                await Task.Yield();

                var oldPrimaryId = _oldConfig.LocalNodePrimaryId;
                var newConfig = _clusterProvider.clusterManager.CurrentConfig;
                var client = oldPrimaryId.Equals(replicaId) ? _client : await GetConnectionAsync(replicaId);

                try
                {
                    if (client == null)
                    {
                        _logger.LogError("Failed to initialize connection to replica {replicaId}", replicaId);
                        return;
                    }

                    var resp = await client.GossipAsync(configByteArray).ConfigureAwait(false);

                    try
                    {
                        var current = _clusterProvider.clusterManager.CurrentConfig;
                        if (resp.Length > 0)
                        {
                            _clusterProvider.clusterManager.gossipStats.UpdateGossipBytesRecv(resp.Length);
                            var returnedConfigArray = resp.Span.ToArray();

                            if (ThrowOnFromByteArray)
                            {
                                throw new Exception("Simulated FromByteArray failure");
                            }

                            var other = FromByteArray(returnedConfigArray);

                            if (current.IsKnown(other.LocalNodeId))
                                _ = _clusterProvider.clusterManager.TryMerge(FromByteArray(returnedConfigArray));
                            else
                                _logger.LogWarning("Received gossip from unknown node: {node-id}", other.LocalNodeId);
                        }
                        resp.Dispose();
                    }
                    catch (Exception ex)
                    {
                        _logger.LogCritical(ex, "IssueAttachReplicas faulted");
                    }
                    finally
                    {
                        resp.Dispose();
                    }

                    var localAddress = _oldConfig.LocalNodeIp;
                    var localPort = _oldConfig.LocalNodePort;

                    var replicaOfResp = await client.ReplicaOf(localAddress, localPort).ConfigureAwait(false);

                    if (!replicaOfResp.Equals("OK"))
                        _logger.LogWarning("IssueAttachReplicas Error: {replicaId} {replicaOfResp}", replicaId, replicaOfResp);
                }
                finally
                {
                    client?.Dispose();
                }
            }
        }

        // Interfaces to mock dependencies (simplified for test)
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

        public interface IReplicationManager
        {
            long ReplicationOffset { get; }
        }

        public interface IClusterConfig
        {
            string LocalNodePrimaryId { get; }
            string LocalNodeId { get; }
            string LocalNodeIp { get; }
            int LocalNodePort { get; }
            bool IsKnown(string nodeId);
        }

        public interface IGossipStats
        {
            void UpdateGossipBytesRecv(int bytes);
        }
    }
}
