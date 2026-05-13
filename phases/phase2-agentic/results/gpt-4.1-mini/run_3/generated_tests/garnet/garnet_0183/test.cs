using System;
using System.Net;
using System.Threading.Tasks;
using Garnet.cluster;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Garnet.Tests.cluster
{
    public class ReplicaSyncSessionTests
    {
        [Fact]
        public async Task SendCheckpointAsync_LogsCheckpointSearchCompleted()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var clusterProviderMock = new Mock<ClusterProvider>();
            var storeWrapperMock = new Mock<StoreWrapper>();

            // Setup clusterProvider to return a clusterManager with CurrentConfig that returns a valid address and port
            var clusterManagerMock = new Mock<IClusterManager>();
            clusterManagerMock.Setup(cm => cm.CurrentConfig.GetWorkerAddressFromNodeId(It.IsAny<string>()))
                .Returns(("127.0.0.1", 12345));
            clusterProviderMock.Setup(cp => cp.clusterManager).Returns(clusterManagerMock.Object);

            // Setup clusterProvider to return dummy replicationManager and serverOptions
            var replicationManagerMock = new Mock<IReplicationManager>();
            clusterProviderMock.Setup(cp => cp.replicationManager).Returns(replicationManagerMock.Object);

            var serverOptionsMock = new Mock<IServerOptions>();
            clusterProviderMock.Setup(cp => cp.serverOptions).Returns(serverOptionsMock.Object);

            // Setup replicaCheckpointEntry with metadata
            var checkpointEntry = new CheckpointEntry
            {
                metadata = new CheckpointMetadata
                {
                    storeVersion = 1,
                    objectStoreVersion = 1,
                    storePrimaryReplId = "primaryId",
                    objectStorePrimaryReplId = "objectPrimaryId",
                    storeHlogToken = new object(),
                    storeIndexToken = new object(),
                    objectStoreHlogToken = new object(),
                    objectStoreIndexToken = new object()
                }
            };

            // Setup replicaSyncSession with minimal required parameters
            var replicaSyncSession = new ReplicaSyncSession(
                storeWrapperMock.Object,
                clusterProviderMock.Object,
                replicaSyncMetadata: null,
                token: default,
                replicaNodeId: "replica1",
                replicaAssignedPrimaryId: null,
                replicaCheckpointEntry: checkpointEntry,
                replicaAofBeginAddress: 0,
                replicaAofTailAddress: 0,
                logger: loggerMock.Object);

            // Setup AcquireCheckpointEntryAsync to return a valid localEntry and null AofSyncTaskInfo
            var localEntry = new CheckpointEntry
            {
                metadata = new CheckpointMetadata
                {
                    storeVersion = 1,
                    objectStoreVersion = 1,
                    storePrimaryReplId = "primaryId",
                    objectStorePrimaryReplId = "objectPrimaryId",
                    storeHlogToken = new object(),
                    storeIndexToken = new object(),
                    objectStoreHlogToken = new object(),
                    objectStoreIndexToken = new object()
                }
            };

            // We need to mock the AcquireCheckpointEntryAsync method, but it's not virtual or interface.
            // So we create a derived class to override it for testing.
            var testSession = new TestReplicaSyncSession(
                storeWrapperMock.Object,
                clusterProviderMock.Object,
                replicaNodeId: "replica1",
                replicaCheckpointEntry: checkpointEntry,
                logger: loggerMock.Object,
                localEntryToReturn: localEntry);

            // Setup ValidateMetadata to always return true
            testSession.SetValidateMetadataResult(true);

            // Setup gcs.ConnectAsync to complete immediately
            testSession.SetConnectAsyncCompleted();

            // Act
            await testSession.SendCheckpointAsync();

            // Assert
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Checkpoint search completed")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.AtLeastOnce);
        }

        // Helper derived class to override async methods for testing
        private class TestReplicaSyncSession : ReplicaSyncSession
        {
            private readonly CheckpointEntry _localEntryToReturn;
            private bool _validateMetadataResult = true;
            private bool _connectAsyncCalled = false;

            public TestReplicaSyncSession(
                StoreWrapper storeWrapper,
                ClusterProvider clusterProvider,
                string replicaNodeId,
                CheckpointEntry replicaCheckpointEntry,
                ILogger logger,
                CheckpointEntry localEntryToReturn)
                : base(storeWrapper, clusterProvider, null, default, replicaNodeId, null, replicaCheckpointEntry, 0, 0, logger)
            {
                _localEntryToReturn = localEntryToReturn;
            }

            public void SetValidateMetadataResult(bool result)
            {
                _validateMetadataResult = result;
            }

            public void SetConnectAsyncCompleted()
            {
                _connectAsyncCalled = true;
            }

            protected override Task<(CheckpointEntry, AofSyncTaskInfo)> AcquireCheckpointEntryAsync()
            {
                return Task.FromResult((_localEntryToReturn, (AofSyncTaskInfo)null));
            }

            public override bool ValidateMetadata(
                CheckpointEntry localEntry,
                out long index_size,
                out LogFileInfo hlog_size,
                out long obj_index_size,
                out LogFileInfo obj_hlog_size,
                out bool skipLocalMainStoreCheckpoint,
                out bool skipLocalObjectStoreCheckpoint)
            {
                index_size = 0;
                hlog_size = default;
                obj_index_size = 0;
                obj_hlog_size = default;
                skipLocalMainStoreCheckpoint = false;
                skipLocalObjectStoreCheckpoint = false;
                return _validateMetadataResult;
            }

            // Override GarnetClientSession creation to return a mock with ConnectAsync overridden
            protected override GarnetClientSession CreateGarnetClientSession(IPEndPoint endpoint)
            {
                var mockGcs = new Mock<GarnetClientSession>(endpoint, null, null, null, null, null, null);
                mockGcs.Setup(g => g.ConnectAsync(It.IsAny<int>())).Returns(Task.CompletedTask);
                return mockGcs.Object;
            }
        }
    }
}
