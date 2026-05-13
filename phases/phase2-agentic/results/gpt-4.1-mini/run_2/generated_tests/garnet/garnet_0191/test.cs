using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Garnet.cluster;
using System.Threading;
using System.Net;
using Garnet.client;
using Garnet.common;
using Garnet.server;

namespace Garnet.Tests
{
    public class ReplicaSyncSessionTests
    {
        // We will test the error logging on line 301 where LogError is called when syncFromAofAddress < storeWrapper.appendOnlyFile.BeginAddress
        // To do this, we need to simulate the conditions that cause this branch to be hit.

        [Fact]
        public async Task SendCheckpointAsync_LogsError_WhenSyncFromAofAddressLessThanBeginAddress_ThrowsException()
        {
            // Arrange
            var mockLogger = new Mock<ILogger>();
            var mockStoreWrapper = new Mock<StoreWrapper>();
            var mockClusterProvider = new Mock<ClusterProvider>();
            var mockReplicationManager = new Mock<ReplicationManager>();
            var mockClusterManager = new Mock<ClusterManager>();
            var mockCurrentConfig = new Mock<ClusterConfig>();
            var mockAppendOnlyFile = new Mock<AppendOnlyFile>();

            // Setup replicaNodeId and replicaCheckpointEntry
            string replicaNodeId = "replica1";

            // Setup AppendOnlyFile BeginAddress to 1000
            mockAppendOnlyFile.SetupGet(a => a.BeginAddress).Returns(1000L);
            mockStoreWrapper.SetupGet(s => s.appendOnlyFile).Returns(mockAppendOnlyFile.Object);

            // Setup serverOptions to disable possibleAofDataLoss condition
            var serverOptions = new ServerOptions
            {
                UseAofNullDevice = false,
                FastAofTruncate = false,
                OnDemandCheckpoint = true,
                ReplicaSyncTimeout = TimeSpan.FromSeconds(5)
            };
            mockStoreWrapper.SetupGet(s => s.serverOptions).Returns(serverOptions);

            // Setup clusterProvider to return the mock replicationManager and serverOptions
            mockClusterProvider.SetupGet(c => c.replicationManager).Returns(mockReplicationManager.Object);
            mockClusterProvider.SetupGet(c => c.serverOptions).Returns(serverOptions);
            mockClusterProvider.SetupGet(c => c.clusterManager).Returns(mockClusterManager.Object);
            mockClusterProvider.SetupGet(c => c.ClusterUsername).Returns("user");
            mockClusterProvider.SetupGet(c => c.ClusterPassword).Returns("pass");

            // Setup clusterManager to return current config
            mockClusterManager.SetupGet(c => c.CurrentConfig).Returns(mockCurrentConfig.Object);

            // Setup currentConfig to return valid IP and port for replicaNodeId
            mockCurrentConfig.Setup(c => c.GetWorkerAddressFromNodeId(replicaNodeId)).Returns(("127.0.0.1", 12345));

            // Setup replicationManager to return PrimaryReplId
            mockReplicationManager.SetupGet(r => r.PrimaryReplId).Returns("primaryReplId");

            // Setup replicationManager to TryAddReplicationTask and TryConnectToReplica to succeed
            mockReplicationManager.Setup(r => r.TryAddReplicationTask(It.IsAny<string>(), It.IsAny<long>(), out It.Ref<AofSyncTaskInfo>.IsAny))
                .Returns(true);
            mockReplicationManager.Setup(r => r.TryConnectToReplica(It.IsAny<string>(), It.IsAny<long>(), It.IsAny<AofSyncTaskInfo>(), out It.Ref<object>.IsAny))
                .Returns(true);

            // Setup AcquireCheckpointEntryAsync to return a valid CheckpointEntry and null AofSyncTaskInfo
            var checkpointEntry = new CheckpointEntry
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

            // We need to create a derived class to override AcquireCheckpointEntryAsync to return our checkpointEntry
            var session = new TestReplicaSyncSession(
                mockStoreWrapper.Object,
                mockClusterProvider.Object,
                replicaSyncMetadata: null,
                token: CancellationToken.None,
                replicaNodeId: replicaNodeId,
                replicaAssignedPrimaryId: null,
                replicaCheckpointEntry: checkpointEntry,
                replicaAofBeginAddress: 0,
                replicaAofTailAddress: 0,
                logger: mockLogger.Object);

            // Setup ValidateMetadata to always return true
            session.OverrideValidateMetadata = (localEntry, out long idxSize, out LogFileInfo hlogSize, out long objIdxSize, out LogFileInfo objHlogSize, out bool skipMain, out bool skipObj) =>
            {
                idxSize = 0;
                hlogSize = default;
                objIdxSize = 0;
                objHlogSize = default;
                skipMain = false;
                skipObj = false;
                return true;
            };

            // Setup ExecuteBeginReplicaRecover to return a string representing a syncFromAofAddress less than BeginAddress (simulate error condition)
            session.OverrideExecuteBeginReplicaRecover = (skipLocalMain, skipLocalObject, replayAof, primaryReplId, localEntryBytes, beginAddress, checkpointAofBeginAddress) =>
            {
                return Task.FromResult("500"); // less than BeginAddress 1000
            };

            // Act & Assert
            var ex = await Assert.ThrowsAsync<Exception>(async () => await session.SendCheckpointAsync());

            // Verify that LogError was called with the expected message and parameters
            mockLogger.Verify(l => l.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("syncFromAofAddress: 500 < beginAofAddress: 1000")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);

            mockLogger.Verify(l => l.LogCheckpointEntry(LogLevel.Error, "Requested replay address truncated", checkpointEntry), Times.Once);

            Assert.Equal("Failed syncing because replica requested truncated AOF address", ex.Message);
        }

        // Helper derived class to override protected/virtual methods for testing
        private class TestReplicaSyncSession : ReplicaSyncSession
        {
            public Func<CheckpointEntry, out long, out LogFileInfo, out long, out LogFileInfo, out bool, out bool, bool> OverrideValidateMetadata;
            public Func<bool, bool, bool, string, byte[], long, long, Task<string>> OverrideExecuteBeginReplicaRecover;

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
                ILogger logger)
                : base(storeWrapper, clusterProvider, replicaSyncMetadata, token, replicaNodeId, replicaAssignedPrimaryId, replicaCheckpointEntry, replicaAofBeginAddress, replicaAofTailAddress, logger)
            {
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
                if (OverrideValidateMetadata != null)
                {
                    return OverrideValidateMetadata(localEntry, out index_size, out hlog_size, out obj_index_size, out obj_hlog_size, out skipLocalMainStoreCheckpoint, out skipLocalObjectStoreCheckpoint);
                }
                return base.ValidateMetadata(localEntry, out index_size, out hlog_size, out obj_index_size, out obj_hlog_size, out skipLocalMainStoreCheckpoint, out skipLocalObjectStoreCheckpoint);
            }

            public Task<string> ExecuteBeginReplicaRecover(
                bool skipLocalMainStoreCheckpoint,
                bool skipLocalObjectStoreCheckpoint,
                bool replayAOF,
                string primaryReplId,
                byte[] localEntryBytes,
                long beginAddress,
                long checkpointAofBeginAddress)
            {
                if (OverrideExecuteBeginReplicaRecover != null)
                {
                    return OverrideExecuteBeginReplicaRecover(skipLocalMainStoreCheckpoint, skipLocalObjectStoreCheckpoint, replayAOF, primaryReplId, localEntryBytes, beginAddress, checkpointAofBeginAddress);
                }
                throw new NotImplementedException();
            }

            // Override AcquireCheckpointEntryAsync to return a fixed checkpoint entry and null AofSyncTaskInfo
            public override Task<(CheckpointEntry, AofSyncTaskInfo)> AcquireCheckpointEntryAsync()
            {
                return Task.FromResult((replicaCheckpointEntry, (AofSyncTaskInfo)null));
            }
        }
    }
}
