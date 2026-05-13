using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Garnet.cluster;
using Garnet.client;
using Garnet.common;
using Garnet.server;

namespace Garnet.Tests.cluster
{
    public class ReplicaSyncSessionTests
    {
        [Fact]
        public async Task SendCheckpointAsync_LogsInformationOnBeginAndCompleteSendingCheckpointMetadata()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var clusterProviderMock = new Mock<ClusterProvider>();
            var storeWrapperMock = new Mock<StoreWrapper>();
            var checkpointEntry = new CheckpointEntry
            {
                metadata = new CheckpointMetadata
                {
                    storePrimaryReplId = "primaryReplId",
                    objectStorePrimaryReplId = "objectPrimaryReplId",
                    storeVersion = 1,
                    objectStoreVersion = 1,
                    storeHlogToken = Guid.NewGuid(),
                    objectStoreHlogToken = Guid.NewGuid()
                }
            };

            // Setup clusterProvider to return dummy checkpoint managers and cluster manager
            var replicationManagerMock = new Mock<ReplicationManager>();
            var clusterManagerMock = new Mock<ClusterManager>();
            var currentConfigMock = new Mock<ClusterConfig>();

            currentConfigMock.Setup(c => c.GetWorkerAddressFromNodeId(It.IsAny<string>()))
                .Returns(("127.0.0.1", 12345));
            clusterManagerMock.SetupGet(c => c.CurrentConfig).Returns(currentConfigMock.Object);
            clusterProviderMock.SetupGet(c => c.clusterManager).Returns(clusterManagerMock.Object);
            clusterProviderMock.SetupGet(c => c.replicationManager).Returns(replicationManagerMock.Object);
            clusterProviderMock.Setup(c => c.GetReplicationLogCheckpointManager(It.IsAny<StoreType>()))
                .Returns(Mock.Of<IReplicationLogCheckpointManager>());

            // Setup serverOptions with ReplicaSyncTimeout
            var serverOptionsMock = new Mock<ServerOptions>();
            serverOptionsMock.SetupGet(o => o.ReplicaSyncTimeout).Returns(TimeSpan.FromSeconds(1));
            clusterProviderMock.SetupGet(c => c.serverOptions).Returns(serverOptionsMock.Object);

            // Setup replicationManager to return dummy network settings and pool
            replicationManagerMock.SetupGet(r => r.GetRSSNetworkBufferSettings).Returns(new NetworkBufferSettings());
            replicationManagerMock.SetupGet(r => r.GetNetworkPool).Returns(new NetworkPool());

            // Setup replicaCheckpointEntry with metadata
            var replicaCheckpointEntry = checkpointEntry;

            // Create the ReplicaSyncSession instance
            var session = new ReplicaSyncSession(
                storeWrapperMock.Object,
                clusterProviderMock.Object,
                replicaCheckpointEntry: replicaCheckpointEntry,
                replicaNodeId: "replicaNodeId",
                logger: loggerMock.Object);

            // Setup AcquireCheckpointEntryAsync to return a valid checkpoint entry and null AofSyncTaskInfo
            var acquireCheckpointEntryAsyncMethod = typeof(ReplicaSyncSession).GetMethod("AcquireCheckpointEntryAsync", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            Assert.NotNull(acquireCheckpointEntryAsyncMethod);
            // We will mock this method by replacing it with a delegate using Moq or by subclassing, but since it's private, we will simulate by reflection or by partial mock.
            // For simplicity, we will create a derived class that overrides this method.

            var testSession = new TestReplicaSyncSession(
                storeWrapperMock.Object,
                clusterProviderMock.Object,
                replicaCheckpointEntry: replicaCheckpointEntry,
                replicaNodeId: "replicaNodeId",
                logger: loggerMock.Object);

            testSession.SetAcquireCheckpointEntryResult(Task.FromResult((replicaCheckpointEntry, (AofSyncTaskInfo)null)));

            // Act
            var result = await testSession.SendCheckpointAsync();

            // Assert
            Assert.True(result);
            loggerMock.Verify(l => l.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("<Begin sending checkpoint metadata")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception, string>>()), Times.AtLeastOnce);

            loggerMock.Verify(l => l.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("<Complete sending checkpoint metadata")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception, string>>()), Times.AtLeastOnce);
        }

        private class TestReplicaSyncSession : ReplicaSyncSession
        {
            private Task<(CheckpointEntry, AofSyncTaskInfo)> acquireCheckpointEntryResult;

            public TestReplicaSyncSession(StoreWrapper storeWrapper, ClusterProvider clusterProvider, SyncMetadata replicaSyncMetadata = null, CancellationToken token = default, string replicaNodeId = null, string replicaAssignedPrimaryId = null, CheckpointEntry replicaCheckpointEntry = null, long replicaAofBeginAddress = 0, long replicaAofTailAddress = 0, ILogger logger = null)
                : base(storeWrapper, clusterProvider, replicaSyncMetadata, token, replicaNodeId, replicaAssignedPrimaryId, replicaCheckpointEntry, replicaAofBeginAddress, replicaAofTailAddress, logger)
            {
            }

            public void SetAcquireCheckpointEntryResult(Task<(CheckpointEntry, AofSyncTaskInfo)> result)
            {
                acquireCheckpointEntryResult = result;
            }

            protected override Task<(CheckpointEntry, AofSyncTaskInfo)> AcquireCheckpointEntryAsync()
            {
                return acquireCheckpointEntryResult ?? base.AcquireCheckpointEntryAsync();
            }
        }
    }
}
