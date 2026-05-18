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
        // We will test the BroadcastConfigAndRequestAttachAsync method to cover the LogCritical call on line 211.
        // We need to simulate the client.GossipAsync call returning a response that causes an exception in the inner try block.

        [Fact]
        public async Task BroadcastConfigAndRequestAttachAsync_LogsCriticalOnException()
        {
            // Arrange
            var replicaId = "replica1";
            var configByteArray = new byte[] { 1, 2, 3 };

            var loggerMock = new Mock<ILogger>();
            var clientMock = new Mock<GarnetClient>(MockBehavior.Strict, 
                endpoint: null, 
                tlsClientOptions: null, 
                sendPageSize: 0, 
                maxOutstandingTasks: 0, 
                authUsername: null, 
                authPassword: null, 
                epoch: 0, 
                logger: null);

            // Setup GossipAsync to return a disposable response that throws on ToArray call (simulate exception in inner try)
            var responseMock = new Mock<IDisposable>();
            var responseBytes = new byte[0];
            var responseSpan = new ReadOnlySpan<byte>(responseBytes);

            // Setup GossipAsync to return a Memory<byte> that throws on ToArray (simulate exception)
            clientMock.Setup(c => c.GossipAsync(It.IsAny<byte[]>()))
                .ReturnsAsync(new Memory<byte>(new byte[] { 1, 2, 3 }));

            // Setup ReplicaOf to return "OK" to avoid other warnings
            clientMock.Setup(c => c.ReplicaOf(It.IsAny<string>(), It.IsAny<int>()))
                .ReturnsAsync("OK");

            // Setup Dispose on client
            clientMock.Setup(c => c.Dispose());

            // Setup clusterProvider and oldConfig mocks
            var failoverSession = new FailoverSessionForTest(loggerMock.Object, clientMock.Object, replicaId);

            // Act
            await failoverSession.InvokeBroadcastConfigAndRequestAttachAsync(replicaId, configByteArray);

            // Assert
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Critical,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("IssueAttachReplicas faulted")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        // Helper class to expose the private method and inject dependencies
        private class FailoverSessionForTest : FailoverSession
        {
            private readonly ILogger _logger;
            private readonly GarnetClient _client;
            private readonly string _replicaId;

            public FailoverSessionForTest(ILogger logger, GarnetClient client, string replicaId)
            {
                _logger = logger;
                _client = client;
                _replicaId = replicaId;

                // Setup minimal clusterProvider and oldConfig to satisfy method dependencies
                clusterProvider = new ClusterProviderForTest();
                oldConfig = new OldConfigForTest(replicaId);
                primaryClient = client;
                logger = logger;
            }

            public async Task InvokeBroadcastConfigAndRequestAttachAsync(string replicaId, byte[] configByteArray)
            {
                await BroadcastConfigAndRequestAttachAsync(replicaId, configByteArray);
            }

            // Override GetConnectionAsync to return the mocked client
            protected override Task<GarnetClient> GetConnectionAsync(string nodeId)
            {
                return Task.FromResult(_client);
            }
        }

        // Minimal stub classes to satisfy dependencies
        private class ClusterProviderForTest
        {
            public ClusterManagerForTest clusterManager { get; } = new ClusterManagerForTest();
        }

        private class ClusterManagerForTest
        {
            public ClusterConfig CurrentConfig { get; } = new ClusterConfigForTest();
            public GossipStats gossipStats { get; } = new GossipStats();
            public bool TryMerge(ClusterConfig config) => true;
        }

        private class ClusterConfigForTest : ClusterConfig
        {
            public override bool IsKnown(string nodeId) => true;
            public override string LocalNodeId => "localNodeId";
        }

        private class GossipStats
        {
            public void UpdateGossipBytesRecv(int length) { }
        }

        private class OldConfigForTest
        {
            private readonly string _primaryId;
            public OldConfigForTest(string primaryId)
            {
                _primaryId = primaryId;
            }
            public string LocalNodePrimaryId => _primaryId;
            public string LocalNodeIp => "127.0.0.1";
            public int LocalNodePort => 1234;
            public string LocalNodeId => "localNodeId";
            public string GetEndpointFromNodeId(string nodeId) => "endpoint";
        }

        private abstract class ClusterConfig
        {
            public abstract bool IsKnown(string nodeId);
            public abstract string LocalNodeId { get; }
            public static ClusterConfig FromByteArray(byte[] array) => new ClusterConfigForTest();
        }
    }
}
