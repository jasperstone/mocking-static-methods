using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Garnet.cluster;
using Garnet.client;

namespace Garnet.Tests.cluster
{
    public class FailoverSessionTests
    {
        // We will test the logging of the warning on line 226 in BroadcastConfigAndRequestAttachAsync
        // which logs when the replicaOfResp is not "OK".

        // To do this, we need to:
        // - Mock the clusterProvider and its dependencies minimally
        // - Mock the GarnetClient to return a non-"OK" response for ReplicaOf
        // - Provide a logger mock to verify LogWarning is called with expected parameters

        // Since FailoverSession is internal partial, we assume we can instantiate it via reflection or
        // create a derived test class with exposed methods for testing.
        // For simplicity, we will create a minimal derived class exposing BroadcastConfigAndRequestAttachAsync.

        private class TestFailoverSession : FailoverSession
        {
            public TestFailoverSession(
                Mock<ILogger> loggerMock,
                Mock<GarnetClient> clientMock,
                string oldPrimaryId,
                string replicaId,
                string replicaOfResponse)
            {
                // Setup minimal required fields via reflection or direct assignment
                this.logger = loggerMock.Object;

                // Setup oldConfig mock with LocalNodePrimaryId and LocalNodeId
                var oldConfigMock = new Mock<IClusterConfig>();
                oldConfigMock.SetupGet(c => c.LocalNodePrimaryId).Returns(oldPrimaryId);
                oldConfigMock.SetupGet(c => c.LocalNodeId).Returns("localNodeId");
                this.oldConfig = oldConfigMock.Object;

                // Setup clusterProvider with clusterManager and replicationManager mocks
                var clusterManagerMock = new Mock<IClusterManager>();
                clusterManagerMock.SetupGet(m => m.CurrentConfig).Returns(new ClusterConfigStub());

                var clusterProviderMock = new Mock<IClusterProvider>();
                clusterProviderMock.SetupGet(p => p.clusterManager).Returns(clusterManagerMock.Object);
                this.clusterProvider = clusterProviderMock.Object;

                // Setup primaryClient and GetConnectionAsync to return the mocked client
                this.primaryClient = clientMock.Object;
                this.GetConnectionAsync = (string nodeId) => Task.FromResult(clientMock.Object);

                // Setup client.GossipAsync to return a dummy response
                clientMock.Setup(c => c.GossipAsync(It.IsAny<byte[]>()))
                    .ReturnsAsync(new ReadOnlyMemory<byte>(new byte[0]));

                // Setup client.ReplicaOf to return the specified replicaOfResponse
                clientMock.Setup(c => c.ReplicaOf(It.IsAny<string>(), It.IsAny<int>()))
                    .ReturnsAsync(replicaOfResponse);

                // Setup Dispose to do nothing
                clientMock.Setup(c => c.Dispose());

                // Setup CancellationTokenSource and failoverTimeout
                this.cts = new CancellationTokenSource();
                this.failoverTimeout = TimeSpan.FromSeconds(1);

                // Setup replicaId for test method call
                this.testReplicaId = replicaId;
            }

            // Expose the method for testing
            public async Task CallBroadcastConfigAndRequestAttachAsync()
            {
                // We call the private method via reflection or make it protected in the test subclass
                await BroadcastConfigAndRequestAttachAsync(testReplicaId, new byte[0]);
            }

            // We need to override GetConnectionAsync to use the delegate
            public Func<string, Task<GarnetClient>> GetConnectionAsync;

            private string testReplicaId;

            // Override GetConnectionAsync method
            private new Task<GarnetClient> GetConnectionAsync(string nodeId)
            {
                return GetConnectionAsync(nodeId);
            }

            // Fields to satisfy base class
            public ILogger logger;
            public IClusterProvider clusterProvider;
            public IClusterConfig oldConfig;
            public GarnetClient primaryClient;
            public CancellationTokenSource cts;
            public TimeSpan failoverTimeout;
        }

        // Stubs for interfaces used in FailoverSession
        public interface IClusterConfig
        {
            string LocalNodePrimaryId { get; }
            string LocalNodeId { get; }
            string GetEndpointFromNodeId(string nodeId);
        }

        public interface IClusterManager
        {
            ClusterConfigStub CurrentConfig { get; }
            bool TryTakeOverForPrimary();
            bool TryMerge(ClusterConfigStub config);
        }

        public interface IClusterProvider
        {
            IClusterManager clusterManager { get; }
            IReplicationManager replicationManager { get; }
            IServerOptions serverOptions { get; }
            string ClusterUsername { get; }
            string ClusterPassword { get; }
            IStoreWrapper storeWrapper { get; }
            Task BumpAndWaitForEpochTransitionAsync();
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

        public interface IServerOptions
        {
            ITlsOptions TlsOptions { get; }
        }

        public interface ITlsOptions
        {
            object TlsClientOptions { get; }
        }

        public interface IStoreWrapper
        {
            void StartPrimaryTasks();
        }

        public enum RecoveryStatus
        {
            ClusterFailover,
            NoRecovery
        }

        public class ClusterConfigStub
        {
            public string LocalNodeId => "localNodeId";
            public string LocalNodePrimaryId => "primaryId";
            public string LocalNodeIp => "127.0.0.1";
            public int LocalNodePort => 1234;

            public string[] GetReplicaIds(string primaryId) => new string[] { "replica1", "replica2" };

            public byte[] ToByteArray() => new byte[0];

            public static ClusterConfigStub FromByteArray(byte[] data) => new ClusterConfigStub();

            public string LocalNodeId => "nodeId";
        }

        [Fact]
        public async Task BroadcastConfigAndRequestAttachAsync_LogsWarning_WhenReplicaOfResponseIsNotOK()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var clientMock = new Mock<GarnetClient>();

            string oldPrimaryId = "primaryId";
            string replicaId = "replica1";
            string replicaOfResponse = "ERROR";

            // Setup client mock to return replicaOfResponse for ReplicaOf call
            clientMock.Setup(c => c.GossipAsync(It.IsAny<byte[]>()))
                .ReturnsAsync(new ReadOnlyMemory<byte>(new byte[0]));
            clientMock.Setup(c => c.ReplicaOf(It.IsAny<string>(), It.IsAny<int>()))
                .ReturnsAsync(replicaOfResponse);
            clientMock.Setup(c => c.Dispose());

            var session = new TestFailoverSession(loggerMock, clientMock, oldPrimaryId, replicaId, replicaOfResponse);

            // Act
            await session.CallBroadcastConfigAndRequestAttachAsync();

            // Assert
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("IssueAttachReplicas Error")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }
    }
}
