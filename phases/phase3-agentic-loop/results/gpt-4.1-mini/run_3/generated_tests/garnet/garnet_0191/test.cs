using System;
using System.Net;
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
        public async Task SendCheckpointAsync_LogsError_WhenSyncFromAofAddressLessThanBeginAddress_AndNoPossibleAofDataLoss()
        {
            // Arrange
            var mockLogger = new Mock<ILogger>();
            var mockStoreWrapper = new Mock<StoreWrapper>();
            var mockClusterProvider = new Mock<ClusterProvider>();

            // Setup appendOnlyFile with BeginAddress = 100
            var mockAppendOnlyFile = new Mock<IAppendOnlyFile>();
            mockAppendOnlyFile.SetupGet(a => a.BeginAddress).Returns(100L);
            mockStoreWrapper.SetupGet(s => s.appendOnlyFile).Returns(mockAppendOnlyFile.Object);

            // Setup serverOptions to disable possibleAofDataLoss
            var mockServerOptions = new Mock<ServerOptions>();
            mockServerOptions.SetupGet(o => o.UseAofNullDevice).Returns(false);
            mockServerOptions.SetupGet(o => o.FastAofTruncate).Returns(true);
            mockServerOptions.SetupGet(o => o.OnDemandCheckpoint).Returns(true);
            mockClusterProvider.SetupGet(c => c.serverOptions).Returns(mockServerOptions.Object);

            // Setup replicationManager.PrimaryReplId
            var mockReplicationManager = new Mock<IReplicationManager>();
            mockReplicationManager.SetupGet(r => r.PrimaryReplId).Returns("primaryReplId");
            mockClusterProvider.SetupGet(c => c.replicationManager).Returns(mockReplicationManager.Object);

            // Setup clusterManager.CurrentConfig.GetWorkerAddressFromNodeId to return valid address and port
            var mockClusterManager = new Mock<IClusterManager>();
            var mockCurrentConfig = new Mock<IClusterConfig>();
            mockCurrentConfig.Setup(c => c.GetWorkerAddressFromNodeId(It.IsAny<string>())).Returns(("127.0.0.1", 12345));
            mockClusterManager.SetupGet(m => m.CurrentConfig).Returns(mockCurrentConfig.Object);
            mockClusterProvider.SetupGet(c => c.clusterManager).Returns(mockClusterManager.Object);

            // Setup replicaCheckpointEntry with metadata to avoid null reference
            var replicaCheckpointEntry = new CheckpointEntry
            {
                metadata = new CheckpointMetadata
                {
                    storePrimaryReplId = "primaryReplId",
                    storeVersion = 1,
                    objectStorePrimaryReplId = "objPrimaryReplId",
                    objectStoreVersion = 1,
                    storeHlogToken = 1,
                    objectStoreHlogToken = 1
                }
            };

            // Setup localEntry with metadata matching replicaCheckpointEntry to pass ValidateMetadata
            var localEntry = new CheckpointEntry
            {
                metadata = new CheckpointMetadata
                {
                    storePrimaryReplId = "primaryReplId",
                    storeVersion = 1,
                    objectStorePrimaryReplId = "objPrimaryReplId",
                    objectStoreVersion = 1,
                    storeHlogToken = 1,
                    objectStoreHlogToken = 1
                }
            };

            // Setup replicationManager.TryAcquireSettledMetadataForMainStore and ObjectStore to return true
            mockReplicationManager.Setup(r => r.TryAcquireSettledMetadataForMainStore(localEntry, out It.Ref<LogFileInfo>.IsAny, out It.Ref<long>.IsAny)).Returns(true);
            mockReplicationManager.Setup(r => r.TryAcquireSettledMetadataForObjectStore(localEntry, out It.Ref<LogFileInfo>.IsAny, out It.Ref<long>.IsAny)).Returns(true);

            // Setup replicationManager methods TryAddReplicationTask and TryConnectToReplica to return true
            mockReplicationManager.Setup(r => r.TryAddReplicationTask(It.IsAny<string>(), It.IsAny<long>(), out It.Ref<AofSyncTaskInfo>.IsAny)).Returns(true);
            mockReplicationManager.Setup(r => r.TryConnectToReplica(It.IsAny<string>(), It.IsAny<long>(), It.IsAny<AofSyncTaskInfo>(), out It.Ref<object>.IsAny)).Returns(true);

            // Create a derived class to override AcquireCheckpointEntryAsync and GarnetClientSession creation
            var testSession = new TestReplicaSyncSession(
                mockStoreWrapper.Object,
                mockClusterProvider.Object,
                replicaCheckpointEntry,
                "replicaNodeId",
                mockLogger.Object,
                localEntry,
                50L, // syncFromAofAddress less than BeginAddress 100
                100L);

            // Act & Assert
            var ex = await Assert.ThrowsAsync<Exception>(() => testSession.SendCheckpointAsync());

            Assert.Equal("Failed syncing because replica requested truncated AOF address", ex.Message);

            // Verify LogError call with expected message
            mockLogger.Verify(l => l.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("syncFromAofAddress: 50 < beginAofAddress: 100")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);

            // Verify LogCheckpointEntry call with LogLevel.Error and message "Requested replay address truncated"
            mockLogger.Verify(l => l.LogCheckpointEntry(LogLevel.Error, "Requested replay address truncated", localEntry), Times.Once);
        }

        // Derived class to override internals for testing
        private class TestReplicaSyncSession : ReplicaSyncSession
        {
            private readonly CheckpointEntry _localEntry;
            private readonly long _syncFromAofAddress;
            private readonly long _beginAddress;

            public TestReplicaSyncSession(
                StoreWrapper storeWrapper,
                ClusterProvider clusterProvider,
                CheckpointEntry replicaCheckpointEntry,
                string replicaNodeId,
                ILogger logger,
                CheckpointEntry localEntry,
                long syncFromAofAddress,
                long beginAddress)
                : base(storeWrapper, clusterProvider, replicaCheckpointEntry: replicaCheckpointEntry, replicaNodeId: replicaNodeId, logger: logger)
            {
                _localEntry = localEntry;
                _syncFromAofAddress = syncFromAofAddress;
                _beginAddress = beginAddress;
            }

            protected override async Task<(CheckpointEntry, AofSyncTaskInfo)> AcquireCheckpointEntryAsync()
            {
                return await Task.FromResult((_localEntry, new AofSyncTaskInfo()));
            }

            protected override async Task<string> ExecuteBeginReplicaRecover(
                bool skipLocalMainStoreCheckpoint,
                bool skipLocalObjectStoreCheckpoint,
                bool replayAOF,
                string primaryReplId,
                byte[] localEntryBytes,
                long beginAddress,
                long checkpointAofBeginAddress)
            {
                // Return syncFromAofAddress as string
                return await Task.FromResult(_syncFromAofAddress.ToString());
            }

            protected override long GetBeginAddress()
            {
                return _beginAddress;
            }
        }
    }
}
