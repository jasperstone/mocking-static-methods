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
        // We test the logging of the warning on line 226 in BroadcastConfigAndRequestAttachAsync
        // which logs when replicaOfResp != "OK".
        // We will mock dependencies to trigger that condition.

        [Fact]
        public async Task BroadcastConfigAndRequestAttachAsync_LogsWarning_WhenReplicaOfRespNotOk()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var clusterManagerMock = new Mock<IClusterManager>();
            var replicationManagerMock = new Mock<IReplicationManager>();
            var clusterProviderMock = new Mock<IClusterProvider>();

            // Setup clusterProvider to return mocks
            clusterProviderMock.SetupGet(c => c.clusterManager).Returns(clusterManagerMock.Object);
            clusterProviderMock.SetupGet(c => c.replicationManager).Returns(replicationManagerMock.Object);
            clusterProviderMock.SetupGet(c => c.ClusterUsername).Returns("user");
            clusterProviderMock.SetupGet(c => c.ClusterPassword).Returns("pass");
            clusterProviderMock.SetupGet(c => c.serverOptions).Returns(new ServerOptions());

            // Setup oldConfig mock
            var oldConfigMock = new Mock<IClusterConfig>();
            oldConfigMock.SetupGet(c => c.LocalNodePrimaryId).Returns("primary");
            oldConfigMock.Setup(c => c.GetEndpointFromNodeId(It.IsAny<string>())).Returns(new System.Net.IPEndPoint(System.Net.IPAddress.Loopback, 1234));
            oldConfigMock.SetupGet(c => c.LocalNodeId).Returns("localNodeId");
            oldConfigMock.SetupGet(c => c.LocalNodeIp).Returns("127.0.0.1");
            oldConfigMock.SetupGet(c => c.LocalNodePort).Returns(1234);

            // Setup newConfig mock
            var newConfigMock = new Mock<IClusterConfig>();
            newConfigMock.Setup(c => c.ToByteArray()).Returns(new byte[] { 1, 2, 3 });
            newConfigMock.Setup(c => c.GetReplicaIds(It.IsAny<string>())).Returns(new System.Collections.Generic.List<string> { "replica1" });

            // Setup clusterManager to return newConfig
            clusterManagerMock.SetupGet(c => c.CurrentConfig).Returns(newConfigMock.Object);

            // Setup replicationManager
            replicationManagerMock.SetupGet(r => r.ReplicationOffset).Returns(0);

            // Setup GarnetClient mock
            var garnetClientMock = new Mock<GarnetClient>(System.Net.IPEndPoint.Loopback, null, 131072, 8, "user", "pass", 0, loggerMock.Object);
            garnetClientMock.Setup(c => c.GossipAsync(It.IsAny<byte[]>())).ReturnsAsync(new ReadOnlyMemory<byte>(new byte[] { 1, 2, 3 }));
            garnetClientMock.Setup(c => c.ReplicaOf(It.IsAny<string>(), It.IsAny<int>())).ReturnsAsync("ERROR");

            // Setup FailoverSession with partial mocks and fields
            var failoverSession = new FailoverSessionPartialMock(
                clusterProviderMock.Object,
                oldConfigMock.Object,
                loggerMock.Object,
                garnetClientMock.Object);

            // Act
            await failoverSession.InvokeBroadcastConfigAndRequestAttachAsync("replica1", newConfigMock.Object.ToByteArray());

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

        // Partial mock class to expose the private method for testing
        private class FailoverSessionPartialMock : FailoverSession
        {
            private readonly GarnetClient _client;

            public FailoverSessionPartialMock(
                IClusterProvider clusterProvider,
                IClusterConfig oldConfig,
                ILogger logger,
                GarnetClient client)
            {
                this.clusterProvider = clusterProvider;
                this.oldConfig = oldConfig;
                this.logger = logger;
                this.primaryClient = client;
                this.failoverTimeout = TimeSpan.FromSeconds(1);
                this.cts = new CancellationTokenSource();
            }

            public async Task InvokeBroadcastConfigAndRequestAttachAsync(string replicaId, byte[] configByteArray)
            {
                await BroadcastConfigAndRequestAttachAsync(replicaId, configByteArray);
            }

            // Expose protected/private members for test
            public new IClusterProvider clusterProvider;
            public new IClusterConfig oldConfig;
            public new ILogger logger;
            public new GarnetClient primaryClient;
            public new TimeSpan failoverTimeout;
            public new CancellationTokenSource cts;
        }

        // Interfaces to mock dependencies (simplified)
        public interface IClusterProvider
        {
            IClusterManager clusterManager { get; }
            IReplicationManager replicationManager { get; }
            ServerOptions serverOptions { get; }
            string ClusterUsername { get; }
            string ClusterPassword { get; }
        }

        public interface IClusterManager
        {
            IClusterConfig CurrentConfig { get; }
            bool TryTakeOverForPrimary();
            bool TryMerge(IClusterConfig config);
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

        public interface IClusterConfig
        {
            string LocalNodePrimaryId { get; }
            string LocalNodeId { get; }
            string LocalNodeIp { get; }
            int LocalNodePort { get; }
            System.Net.IPEndPoint GetEndpointFromNodeId(string nodeId);
            byte[] ToByteArray();
            System.Collections.Generic.List<string> GetReplicaIds(string primaryId);
        }

        public class ServerOptions
        {
            public TlsOptions TlsOptions { get; set; }
        }

        public class TlsOptions
        {
            public object TlsClientOptions { get; set; }
        }

        public enum RecoveryStatus
        {
            NoRecovery,
            ClusterFailover
        }
    }
}
