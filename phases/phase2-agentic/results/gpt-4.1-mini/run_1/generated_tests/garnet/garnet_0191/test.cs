using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Garnet.cluster;
using Garnet.common;
using Garnet.server;

namespace Garnet.Tests.cluster
{
    public class ReplicaSyncSessionTests
    {
        [Fact]
        public async Task SendCheckpointAsync_LogsErrorWhenSyncFromAofAddressLessThanBeginAddress_ThrowsException()
        {
            // Arrange
            var mockLogger = new Mock<ILogger>();
            var mockClusterProvider = new Mock<ClusterProvider>();
            var mockReplicationManager = new Mock<ReplicationManager>();
            var mockStoreWrapper = new Mock<StoreWrapper>();
            var mockAppendOnlyFile = new Mock<AppendOnlyFile>();

            // Setup storeWrapper and appendOnlyFile with BeginAddress
            mockAppendOnlyFile.SetupGet(a => a.BeginAddress).Returns(100L);
            mockStoreWrapper.SetupGet(s => s.appendOnlyFile).Returns(mockAppendOnlyFile.Object);
            mockStoreWrapper.SetupGet(s => s.serverOptions).Returns(new ServerOptions
            {
                UseAofNullDevice = false,
                FastAofTruncate = false,
                OnDemandCheckpoint = true,
                ReplicaSyncTimeout = TimeSpan.FromSeconds(5)
            });

            // Setup clusterProvider and replicationManager
            mockClusterProvider.SetupGet(c => c.replicationManager).Returns(mockReplicationManager.Object);
            mockClusterProvider.SetupGet(c => c.serverOptions).Returns(mockStoreWrapper.Object.serverOptions);

            // Setup replicationManager.PrimaryReplId
            mockReplicationManager.SetupGet(r => r.PrimaryReplId).Returns("primaryReplId");

            // Setup replicationManager.TryAddReplicationTask and TryConnectToReplica to succeed
            mockReplicationManager.Setup(r => r.TryAddReplicationTask(It.IsAny<string>(), It.IsAny<long>(), out It.Ref<AofSyncTaskInfo>.IsAny))
                .Returns(true);
            mockReplicationManager.Setup(r => r.TryConnectToReplica(It.IsAny<string>(), It.IsAny<long>(), It.IsAny<AofSyncTaskInfo>(), out It.Ref<object>.IsAny))
                .Returns(true);

            // Setup clusterManager.CurrentConfig.GetWorkerAddressFromNodeId to return valid address and port
            var mockClusterManager = new Mock<ClusterManager>();
            mockClusterManager.Setup(c => c.CurrentConfig).Returns(new ClusterConfig());
            mockClusterManager.Setup(c => c.CurrentConfig.GetWorkerAddressFromNodeId(It.IsAny<string>())).Returns(("127.0.0.1", 12345));
            mockClusterProvider.SetupGet(c => c.clusterManager).Returns(mockClusterManager.Object);

            // Setup replicaCheckpointEntry with metadata
            var replicaCheckpointEntry = new CheckpointEntry
            {
                metadata = new CheckpointMetadata
                {
                    storePrimaryReplId = "primaryReplId",
                    storeVersion = 1,
                    objectStorePrimaryReplId = "objPrimaryReplId",
                    objectStoreVersion = 1,
                    storeHlogToken = new HlogToken(),
                    objectStoreHlogToken = new HlogToken()
                }
            };

            // Setup SyncMetadata and other parameters
            var replicaSyncMetadata = new SyncMetadata();

            // Create the ReplicaSyncSession instance with mocks
            var session = new ReplicaSyncSession(
                mockStoreWrapper.Object,
                mockClusterProvider.Object,
                replicaSyncMetadata,
                default,
                "replicaNodeId",
                "replicaAssignedPrimaryId",
                replicaCheckpointEntry,
                0,
                0,
                mockLogger.Object);

            // We need to mock or override AcquireCheckpointEntryAsync to return a localEntry with metadata
            var localEntry = new CheckpointEntry
            {
                metadata = new CheckpointMetadata
                {
                    storePrimaryReplId = "primaryReplId",
                    storeVersion = 1,
                    objectStorePrimaryReplId = "objPrimaryReplId",
                    objectStoreVersion = 1,
                    storeHlogToken = new HlogToken(),
                    objectStoreHlogToken = new HlogToken()
                }
            };

            // Setup ValidateMetadata to return true
            // We will override ValidateMetadata by subclassing for test
            var testSession = new TestReplicaSyncSession(
                mockStoreWrapper.Object,
                mockClusterProvider.Object,
                replicaSyncMetadata,
                default,
                "replicaNodeId",
                "replicaAssignedPrimaryId",
                replicaCheckpointEntry,
                0,
                0,
                mockLogger.Object,
                localEntry);

            // Act & Assert
            var ex = await Assert.ThrowsAsync<Exception>(() => testSession.SendCheckpointAsync());

            // Verify that LogError was called with the expected message
            mockLogger.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("syncFromAofAddress")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.AtLeastOnce);

            Assert.Equal("Failed syncing because replica requested truncated AOF address", ex.Message);
        }

        // Helper subclass to override async methods for testing
        private class TestReplicaSyncSession : ReplicaSyncSession
        {
            private readonly CheckpointEntry _localEntry;

            public TestReplicaSyncSession(
                StoreWrapper storeWrapper,
                ClusterProvider clusterProvider,
                SyncMetadata replicaSyncMetadata,
                CancellationToken token,
                string replicaNodeId,
                string replicaAssignedPrimaryId,
                CheckpointEntry replicaCheckpointEntry,
                long replicaAofBeginAddress,
                long replicaAofTailAddress,
                ILogger logger,
                CheckpointEntry localEntry)
                : base(storeWrapper, clusterProvider, replicaSyncMetadata, token, replicaNodeId, replicaAssignedPrimaryId, replicaCheckpointEntry, replicaAofBeginAddress, replicaAofTailAddress, logger)
            {
                _localEntry = localEntry;
            }

            protected override Task<(CheckpointEntry, AofSyncTaskInfo)> AcquireCheckpointEntryAsync()
            {
                // Return the localEntry and a dummy AofSyncTaskInfo
                return Task.FromResult((_localEntry, new AofSyncTaskInfo()));
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
                // Always return true for test
                index_size = 0;
                hlog_size = default;
                obj_index_size = 0;
                obj_hlog_size = default;
                skipLocalMainStoreCheckpoint = false;
                skipLocalObjectStoreCheckpoint = false;
                return true;
            }

            public override async Task<string> ExecuteBeginReplicaRecoverAsync(
                bool skipLocalMainStoreCheckpoint,
                bool skipLocalObjectStoreCheckpoint,
                bool replayAOF,
                string primaryReplId,
                byte[] localEntryBytes,
                long beginAddress,
                long checkpointAofBeginAddress)
            {
                // Return a string representing a syncFromAofAddress less than BeginAddress to trigger the error path
                return "50"; // less than BeginAddress 100
            }

            public override Task ConnectGarnetClientSessionAsync(GarnetClientSession gcs)
            {
                // No-op for test
                return Task.CompletedTask;
            }
        }
    }
}
