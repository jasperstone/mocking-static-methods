using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using System.Threading;
using Garnet.cluster;
using Garnet.common;
using Garnet.server;
using System.Net;
using System;

namespace Garnet.Tests
{
    public class ReplicaSyncSessionTests
    {
        [Fact]
        public async Task SendCheckpointAsync_LogsRequestingCheckpoint()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var storeWrapperMock = new Mock<StoreWrapper>();
            var clusterProviderMock = new Mock<ClusterProvider>();
            var clusterManagerMock = new Mock<ClusterManager>();
            var replicationManagerMock = new Mock<ReplicationManager>();
            var currentConfigMock = new Mock<ClusterConfig>();
            var serverOptions = new ServerOptions
            {
                TlsOptions = null,
                ReplicaSyncTimeout = TimeSpan.FromSeconds(1)
            };
            var checkpointEntry = new CheckpointEntry
            {
                metadata = new CheckpointMetadata
                {
                    storeVersion = 1,
                    objectStoreVersion = 1,
                    storePrimaryReplId = "id",
                    objectStorePrimaryReplId = "id"
                }
            };

            // Setup minimal dependencies
            storeWrapperMock.SetupGet(s => s.serverOptions).Returns(serverOptions);
            clusterProviderMock.SetupGet(c => c.replicationManager).Returns(replicationManagerMock.Object);
            clusterProviderMock.SetupGet(c => c.clusterManager).Returns(clusterManagerMock.Object);
            clusterProviderMock.SetupGet(c => c.serverOptions).Returns(serverOptions);
            clusterProviderMock.SetupGet(c => c.clusterManager.CurrentConfig).Returns(currentConfigMock.Object);
            currentConfigMock.Setup(c => c.GetWorkerAddressFromNodeId(It.IsAny<string>()))
                .Returns(("127.0.0.1", 12345));
            clusterProviderMock.SetupGet(c => c.ClusterUsername).Returns("user");
            clusterProviderMock.SetupGet(c => c.ClusterPassword).Returns("pass");
            clusterProviderMock.SetupGet(c => c.replicationManager).Returns(replicationManagerMock.Object);
            clusterProviderMock.SetupGet(c => c.serverOptions).Returns(new ServerOptions());

            var session = new ReplicaSyncSession(
                storeWrapperMock.Object,
                clusterProviderMock.Object,
                replicaSyncMetadata: null,
                token: CancellationToken.None,
                replicaNodeId: "node1",
                replicaAssignedPrimaryId: "primary1",
                replicaCheckpointEntry: checkpointEntry,
                logger: loggerMock.Object
            );

            // Mock AcquireCheckpointEntryAsync to return dummy data
            var dummyLocalEntry = new CheckpointEntry
            {
                metadata = new CheckpointMetadata
                {
                    storeVersion = 1,
                    objectStoreVersion = 1,
                    storePrimaryReplId = "id",
                    objectStorePrimaryReplId = "id"
                }
            };
            var dummyAofSyncTaskInfo = new AofSyncTaskInfo();

            // Use reflection to set the private method's return value
            var method = typeof(ReplicaSyncSession).GetMethod("AcquireCheckpointEntryAsync", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            // Since we can't override private methods easily, we will simulate the call by calling SendCheckpointAsync and mocking dependencies accordingly.

            // Act
            await session.SendCheckpointAsync();

            // Assert
            // Verify that LogInformation was called with the expected message
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("requesting checkpoint")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.AtLeastOnce);
        }
    }
}
