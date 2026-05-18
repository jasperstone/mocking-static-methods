using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using Garnet.cluster;
using Garnet.client;
using System.Threading.Tasks;
using System.Threading;
using System;

namespace Garnet.Tests
{
    public class ReplicaFailoverSessionTests
    {
        [Fact]
        public async Task BroadcastConfigAndRequestAttachAsync_LogsWarning_WhenReplicaOfRespIsNotOK()
        {
            // Arrange
            var mockLogger = new Mock<ILogger<FailoverSession>>();
            var mockClient = new Mock<GarnetClient>();
            var mockClusterProvider = new Mock<ClusterProvider>();
            var mockClusterManager = new Mock<ClusterManager>();
            var mockReplicationManager = new Mock<ReplicationManager>();
            var mockStoreWrapper = new Mock<StoreWrapper>();
            var mockOldConfig = new Mock<ClusterConfig>();
            var mockNewConfig = new Mock<ClusterConfig>();
            var mockGossipStats = new Mock<GossipStats>();
            var mockTlsOptions = new Mock<TlsOptions>();
            var mockTlsClientOptions = new Mock<TlsClientOptions>();
            var mockCmdStrings = new Mock<CmdStrings>();
            var mockExceptionInjectionHelper = new Mock<ExceptionInjectionHelper>();
            var mockRecoveryStatus = new Mock<RecoveryStatus>();
            var mockFailoverStatus = new Mock<FailoverStatus>();
            var mockEpoch = new Mock<Epoch>();
            var mockCts = new Mock<CancellationTokenSource>();

            mockClusterProvider.Setup(cp => cp.clusterManager).Returns(mockClusterManager.Object);
            mockClusterProvider.Setup(cp => cp.replicationManager).Returns(mockReplicationManager.Object);
            mockClusterProvider.Setup(cp => cp.storeWrapper).Returns(mockStoreWrapper.Object);
            mockClusterProvider.Setup(cp => cp.serverOptions.TlsOptions).Returns(mockTlsOptions.Object);
            mockTlsOptions.Setup(to => to.TlsClientOptions).Returns(mockTlsClientOptions.Object);
            mockClusterProvider.Setup(cp => cp.ClusterUsername).Returns("username");
            mockClusterProvider.Setup(cp => cp.ClusterPassword).Returns("password");
            mockClusterProvider.Setup(cp => cp.epoch).Returns(mockEpoch.Object);
            mockClusterProvider.Setup(cp => cp.logger).Returns(mockLogger.Object);
            mockClusterProvider.Setup(cp => cp.clusterManager.gossipStats).Returns(mockGossipStats.Object);
            mockClusterProvider.Setup(cp => cp.clusterManager.CurrentConfig).Returns(mockNewConfig.Object);
            mockClusterProvider.Setup(cp => cp.replicationManager.ReplicationOffset).Returns(0);
            mockClusterProvider.Setup(cp => cp.replicationManager.BeginRecovery(RecoveryStatus.ClusterFailover, false)).Returns(true);
            mockClusterProvider.Setup(cp => cp.replicationManager.EndRecovery(RecoveryStatus.NoRecovery, false));
            mockClusterProvider.Setup(cp => cp.replicationManager.ResetReplayIterator());
            mockClusterProvider.Setup(cp => cp.replicationManager.InitializeCheckpointStore()).Returns(true);
            mockClusterProvider.Setup(cp => cp.storeWrapper.StartPrimaryTasks());
            mockClusterProvider.Setup(cp => cp.BumpAndWaitForEpochTransitionAsync()).Returns(Task.CompletedTask);
            mockClusterProvider.Setup(cp => cp.replicationManager.TryUpdateForFailover());
            mockClusterProvider.Setup(cp => cp.clusterManager.TryTakeOverForPrimary()).Returns(true);

            mockOldConfig.Setup(oc => oc.LocalNodePrimaryId).Returns("primaryId");
            mockOldConfig.Setup(oc => oc.LocalNodeId).Returns("localNodeId");
            mockOldConfig.Setup(oc => oc.LocalNodeIp).Returns("127.0.0.1");
            mockOldConfig.Setup(oc => oc.LocalNodePort).Returns(1234);
            mockOldConfig.Setup(oc => oc.GetEndpointFromNodeId(It.IsAny<string>())).Returns(new Endpoint("127.0.0.1", 1234));

            mockNewConfig.Setup(nc => nc.GetReplicaIds(It.IsAny<string>())).Returns(new List<string> { "replicaId" });
            mockNewConfig.Setup(nc => nc.ToByteArray()).Returns(new byte[] { });

            mockClient.Setup(c => c.GossipAsync(It.IsAny<byte[]>())).Returns(Task.FromResult(new Response()));
            mockClient.Setup(c => c.ReplicaOf(It.IsAny<string>(), It.IsAny<int>())).Returns(Task.FromResult("NOT_OK"));

            var failoverSession = new FailoverSession(
                mockClusterProvider.Object,
                mockOldConfig.Object,
                mockNewConfig.Object,
                mockLogger.Object,
                mockCts.Object.Token,
                TimeSpan.FromSeconds(30)
            );

            // Act
            await failoverSession.BroadcastConfigAndRequestAttachAsync("replicaId", new byte[] { });

            // Assert
            mockLogger.Verify(
                x => x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("IssueAttachReplicas Error: replicaId NOT_OK")),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
                Times.Once);
        }
    }
}
