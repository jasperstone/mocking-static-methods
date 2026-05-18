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
            var clusterProviderMock = new Mock<ClusterProvider>(MockBehavior.Strict);
            var storeWrapperMock = new Mock<StoreWrapper>(MockBehavior.Strict);

            // Setup minimal required properties and methods for clusterProvider and storeWrapper
            var serverOptionsMock = new Mock<ServerOptions>();
            serverOptionsMock.SetupGet(o => o.ReplicaSyncTimeout).Returns(TimeSpan.FromSeconds(1));
            var replicationManagerMock = new Mock<ReplicationManager>();
            var clusterManagerMock = new Mock<ClusterManager>();
            var currentConfigMock = new Mock<ClusterConfig>();

            currentConfigMock.Setup(c => c.GetWorkerAddressFromNodeId(It.IsAny<string>())).Returns(("127.0.0.1", 12345));
            clusterManagerMock.SetupGet(c => c.CurrentConfig).Returns(currentConfigMock.Object);
            clusterProviderMock.SetupGet(c => c.serverOptions).Returns(serverOptionsMock.Object);
            clusterProviderMock.SetupGet(c => c.replicationManager).Returns(replicationManagerMock.Object);
            clusterProviderMock.SetupGet(c => c.clusterManager).Returns(clusterManagerMock.Object);
            clusterProviderMock.SetupGet(c => c.ClusterUsername).Returns("user");
            clusterProviderMock.SetupGet(c => c.ClusterPassword).Returns("pass");
            clusterProviderMock.Setup(c => c.GetReplicationLogCheckpointManager(It.IsAny<StoreType>())).Returns(Mock.Of<IReplicationLogCheckpointManager>());

            storeWrapperMock.SetupGet(s => s.serverOptions).Returns(serverOptionsMock.Object);

            // Setup replicaCheckpointEntry with metadata to avoid null refs
            var checkpointMetadata = new CheckpointEntry
            {
                metadata = new CheckpointMetadata
                {
                    storePrimaryReplId = "primaryId",
                    objectStorePrimaryReplId = "objPrimaryId",
                    storeVersion = 1,
                    objectStoreVersion = 1,
                    storeHlogToken = Guid.NewGuid(),
                    objectStoreHlogToken = Guid.NewGuid()
                }
            };

            // Create the ReplicaSyncSession instance with mocks and test data
            var session = new ReplicaSyncSession(
                storeWrapperMock.Object,
                clusterProviderMock.Object,
                replicaCheckpointEntry: checkpointMetadata,
                replicaNodeId: "replica1",
                logger: loggerMock.Object);

            // We need to mock AcquireCheckpointEntryAsync to return a valid localEntry and null AofSyncTaskInfo
            var localEntry = new CheckpointEntry
            {
                metadata = new CheckpointMetadata
                {
                    storePrimaryReplId = "primaryId",
                    objectStorePrimaryReplId = "objPrimaryId",
                    storeVersion = 1,
                    objectStoreVersion = 1,
                    storeHlogToken = Guid.NewGuid(),
                    objectStoreHlogToken = Guid.NewGuid()
                }
            };

            // Use reflection to replace the private method AcquireCheckpointEntryAsync with a delegate returning our localEntry
            var method = typeof(ReplicaSyncSession).GetMethod("AcquireCheckpointEntryAsync", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            Assert.NotNull(method);
            var task = Task.FromResult((localEntry, (AofSyncTaskInfo)null));
            var func = new Func<Task<(CheckpointEntry, AofSyncTaskInfo)>>(() => task);
            // We cannot replace private methods easily, so we will create a derived test class to override it

            var testSession = new TestReplicaSyncSession(
                storeWrapperMock.Object,
                clusterProviderMock.Object,
                replicaCheckpointEntry: checkpointMetadata,
                replicaNodeId: "replica1",
                logger: loggerMock.Object,
                acquireCheckpointEntryAsync: func);

            // Act
            var result = await testSession.SendCheckpointAsync();

            // Assert
            Assert.True(result);

            // Verify that LogInformation was called with the expected messages including the line 463 call
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
            private readonly Func<Task<(CheckpointEntry, AofSyncTaskInfo)>> _acquireCheckpointEntryAsync;

            public TestReplicaSyncSession(
                StoreWrapper storeWrapper,
                ClusterProvider clusterProvider,
                SyncMetadata replicaSyncMetadata = null,
                CancellationToken token = default,
                string replicaNodeId = null,
                string replicaAssignedPrimaryId = null,
                CheckpointEntry replicaCheckpointEntry = null,
                long replicaAofBeginAddress = 0,
                long replicaAofTailAddress = 0,
                ILogger logger = null,
                Func<Task<(CheckpointEntry, AofSyncTaskInfo)>> acquireCheckpointEntryAsync = null)
                : base(storeWrapper, clusterProvider, replicaSyncMetadata, token, replicaNodeId, replicaAssignedPrimaryId, replicaCheckpointEntry, replicaAofBeginAddress, replicaAofTailAddress, logger)
            {
                _acquireCheckpointEntryAsync = acquireCheckpointEntryAsync;
            }

            protected override Task<(CheckpointEntry, AofSyncTaskInfo)> AcquireCheckpointEntryAsync()
            {
                return _acquireCheckpointEntryAsync != null ? _acquireCheckpointEntryAsync() : base.AcquireCheckpointEntryAsync();
            }
        }
    }
}
