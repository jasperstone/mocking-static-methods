using System;
using System.Threading;
using System.Threading.Tasks;
using Garnet.client;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Garnet.cluster
{
    public class FailoverSessionTests
    {
        [Fact]
        public async Task BroadcastConfigAndRequestAttachAsync_LogsWarning_WhenReplicaOfRespNotOk()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var failoverSession = new TestFailoverSession(loggerMock.Object);

            failoverSession.Setup();

            // Act
            await failoverSession.InvokeBroadcastConfigAndRequestAttachAsync("replica1", new byte[] { 1, 2, 3 });

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

        // Subclass to expose private method and setup dependencies
        private class TestFailoverSession : FailoverSession
        {
            private readonly ILogger _logger;

            public TestFailoverSession(ILogger logger)
            {
                _logger = logger;
                this.logger = logger;
            }

            public void Setup()
            {
                oldConfig = new TestOldConfig();
                clusterProvider = new TestClusterProvider();
                cts = new CancellationTokenSource();
                failoverTimeout = TimeSpan.FromSeconds(1);

                var clientMock = new Mock<GarnetClient>(null, null, 0, 0, null, null, 0, _logger);
                clientMock.Setup(c => c.GossipAsync(It.IsAny<byte[]>()))
                    .ReturnsAsync(new ReadOnlyMemory<byte>(new byte[] { 1, 2, 3 }));
                clientMock.Setup(c => c.ReplicaOf(It.IsAny<string>(), It.IsAny<int>()))
                    .ReturnsAsync("ERROR");

                primaryClient = clientMock.Object;
            }

            public async Task InvokeBroadcastConfigAndRequestAttachAsync(string replicaId, byte[] configByteArray)
            {
                await BroadcastConfigAndRequestAttachAsync(replicaId, configByteArray);
            }
        }

        private class TestOldConfig
        {
            public string LocalNodePrimaryId => "primary1";
            public string LocalNodeId => "localnode1";
            public string GetEndpointFromNodeId(string nodeId) => "endpoint";
        }

        private class TestClusterProvider
        {
            public TestClusterManager clusterManager = new TestClusterManager();
            public TestReplicationManager replicationManager = new TestReplicationManager();
            public TestServerOptions serverOptions = new TestServerOptions();
            public string ClusterUsername => "user";
            public string ClusterPassword => "pass";
            public TestStoreWrapper storeWrapper = new TestStoreWrapper();

            public Task BumpAndWaitForEpochTransitionAsync() => Task.CompletedTask;
        }

        private class TestClusterManager
        {
            public TestClusterConfig CurrentConfig => new TestClusterConfig();
            public TestGossipStats gossipStats = new TestGossipStats();

            public bool TryTakeOverForPrimary() => true;
            public bool TryMerge(TestClusterConfig config) => true;
        }

        private class TestClusterConfig
        {
            public string LocalNodeIp => "127.0.0.1";
            public int LocalNodePort => 1234;
            public string LocalNodePrimaryId => "primary1";

            public byte[] ToByteArray() => new byte[] { 1, 2, 3 };
            public string[] GetReplicaIds(string oldPrimaryId) => new string[] { "replica1" };

            public static TestClusterConfig FromByteArray(byte[] array) => new TestClusterConfig();
            public string LocalNodeId => "localnode1";
        }

        private class TestGossipStats
        {
            public void UpdateGossipBytesRecv(int length) { }
        }

        private class TestReplicationManager
        {
            public long ReplicationOffset => 0;

            public bool BeginRecovery(RecoveryStatus status, bool upgradeLock) => true;
            public void EndRecovery(RecoveryStatus status, bool downgradeLock) { }
            public void TryUpdateForFailover() { }
            public void ResetReplayIterator() { }
            public bool InitializeCheckpointStore() => true;
        }

        private class TestServerOptions
        {
            public TestTlsOptions TlsOptions => new TestTlsOptions();
        }

        private class TestTlsOptions
        {
            public object TlsClientOptions => null;
        }

        private class TestStoreWrapper
        {
            public void StartPrimaryTasks() { }
        }
    }
}
