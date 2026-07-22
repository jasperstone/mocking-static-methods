using System;
using System.Net;
using System.Threading.Tasks;
using Garnet.cluster;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Garnet.Tests
{
    public class ReplicaSyncSessionTests
    {
        [Fact]
        public async Task SendCheckpointAsync_LogsInformationCall()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var clusterProviderMock = new Mock<ClusterProvider>();
            var storeWrapperMock = new Mock<StoreWrapper>();
            var clusterConfigMock = new Mock<ClusterConfig>();
            var clusterManagerMock = new Mock<ClusterManager>();
            var replicationManagerMock = new Mock<ReplicationManager>();
            var serverOptionsMock = new Mock<ServerOptions>();
            var tlsOptionsMock = new Mock<TlsOptions>();
            var networkBufferSettings = new object();
            var networkPool = new object();

            // Setup clusterProvider mock
            clusterProviderMock.Setup(cp => cp.GetReplicationLogCheckpointManager(It.IsAny<StoreType>()))
                .Returns(new Mock<ReplicationLogCheckpointManager>().Object);
            clusterProviderMock.Setup(cp => cp.clusterManager).Returns(clusterManagerMock.Object);
            clusterProviderMock.Setup(cp => cp.replicationManager).Returns(replicationManagerMock.Object);
            clusterProviderMock.Setup(cp => cp.serverOptions).Returns(serverOptionsMock.Object);
            clusterProviderMock.Setup(cp => cp.ClusterUsername).Returns("user");
            clusterProviderMock.Setup(cp => cp.ClusterPassword).Returns("pass");
            clusterProviderMock.Setup(cp => cp.clusterManager.CurrentConfig).Returns(new Mock<ClusterConfig>().Object);

            // Setup cluster config mock
            var currentConfigMock = new Mock<ClusterConfig>();
            currentConfigMock.Setup(c => c.GetWorkerAddressFromNodeId(It.IsAny<string>()))
                .Returns((string address, int port) => ("127.0.0.1", 1234));
            clusterProviderMock.Setup(cp => cp.clusterManager.CurrentConfig).Returns(currentConfigMock.Object);

            // Setup replication manager mock
            replicationManagerMock.Setup(rm => rm.GetRSSNetworkBufferSettings).Returns(networkBufferSettings);
            replicationManagerMock.Setup(rm => rm.GetNetworkPool).Returns(networkPool);

            // Setup server options mock
            serverOptionsMock.Setup(so => so.TlsOptions).Returns(tlsOptionsMock.Object);
            serverOptionsMock.Setup(so => so.ReplicaSyncTimeout).Returns(TimeSpan.FromSeconds(1));
            serverOptionsMock.Setup(so => so.EnableStorageTier).Returns(false);
            serverOptionsMock.Setup(so => so.DisableObjects).Returns(false);

            // Create the ReplicaSyncSession instance
            var session = new ReplicaSyncSession(
                storeWrapperMock.Object,
                clusterProviderMock.Object,
                replicaNodeId: "node1",
                logger: loggerMock.Object);

            // Mock AcquireCheckpointEntryAsync to return dummy data
            async Task<(CheckpointEntry, AofSyncTaskInfo)> DummyAcquireCheckpointAsync()
            {
                await Task.Delay(10);
                var checkpointEntry = new CheckpointEntry
                {
                    metadata = new CheckpointMetadata
                    {
                        storeVersion = 1,
                        objectStoreVersion = 1,
                        storeHlogToken = "token",
                        storeIndexToken = "indexToken"
                    }
                };
                return (checkpointEntry, null);
            }

            // Use reflection to invoke SendCheckpointAsync
            var method = typeof(ReplicaSyncSession).GetMethod("SendCheckpointAsync");
            var task = (Task<bool>)method.Invoke(session, null);
            await task;

            // Assert
            loggerMock.Verify(
                x => x.LogInformation(It.Is<string>(msg => msg.Contains("requesting checkpoint")), 
                It.IsAny<object[]>()),
                Times.AtLeastOnce);
        }
    }
}
