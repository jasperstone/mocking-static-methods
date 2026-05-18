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
        public async Task SendCheckpointAsync_LogsCheckpointSearchCompleted()
        {
            // Arrange
            var mockLogger = new Mock<ILogger>();
            var mockClusterProvider = new Mock<ClusterProvider>();
            var mockClusterManager = new Mock<ClusterManager>();
            var mockCurrentConfig = new Mock<CurrentConfig>();
            var mockReplicationManager = new Mock<ReplicationManager>();
            var mockStoreWrapper = new Mock<StoreWrapper>();
            var mockServerOptions = new Mock<ServerOptions>();
            var mockTlsOptions = new Mock<TlsOptions>();
            var mockCheckpointEntry = new CheckpointEntry
            {
                metadata = new Metadata
                {
                    storeVersion = 1,
                    objectStoreVersion = 1,
                    storePrimaryReplId = "primary1",
                    objectStorePrimaryReplId = "primary1",
                    storeHlogToken = new Token(),
                    objectStoreHlogToken = new Token(),
                    storeIndexToken = new Token(),
                    objectStoreIndexToken = new Token()
                }
            };

            // Setup clusterProvider and related mocks
            mockClusterProvider.SetupGet(p => p.clusterManager).Returns(mockClusterManager.Object);
            mockClusterProvider.SetupGet(p => p.replicationManager).Returns(mockReplicationManager.Object);
            mockClusterProvider.SetupGet(p => p.serverOptions).Returns(mockServerOptions.Object);
            mockClusterProvider.SetupGet(p => p.ClusterUsername).Returns("user");
            mockClusterProvider.SetupGet(p => p.ClusterPassword).Returns("pass");

            mockClusterManager.SetupGet(m => m.CurrentConfig).Returns(mockCurrentConfig.Object);
            mockCurrentConfig.Setup(c => c.GetWorkerAddressFromNodeId(It.IsAny<string>())).Returns(("127.0.0.1", 12345));

            mockReplicationManager.SetupGet(r => r.GetRSSNetworkBufferSettings).Returns(() => null);
            mockReplicationManager.SetupGet(r => r.GetNetworkPool).Returns(() => null);

            mockServerOptions.SetupGet(s => s.TlsOptions).Returns(mockTlsOptions.Object);
            mockTlsOptions.SetupGet(t => t.TlsClientOptions).Returns(() => null);
            mockServerOptions.SetupGet(s => s.ReplicaSyncTimeout).Returns(TimeSpan.FromSeconds(1));
            mockServerOptions.SetupGet(s => s.EnableStorageTier).Returns(false);
            mockServerOptions.SetupGet(s => s.DisableObjects).Returns(false);

            // Setup StoreWrapper
            mockStoreWrapper.SetupGet(s => s.serverOptions).Returns(mockServerOptions.Object);

            // Setup AcquireCheckpointEntryAsync to return a valid localEntry and null AofSyncTaskInfo
            var localEntry = new CheckpointEntry
            {
                metadata = new Metadata
                {
                    storeVersion = 1,
                    objectStoreVersion = 1,
                    storePrimaryReplId = "primary1",
                    objectStorePrimaryReplId = "primary1",
                    storeHlogToken = new Token(),
                    objectStoreHlogToken = new Token(),
                    storeIndexToken = new Token(),
                    objectStoreIndexToken = new Token()
                }
            };

            // We need to create a derived class to override AcquireCheckpointEntryAsync for testing
            var session = new TestReplicaSyncSession(
                mockStoreWrapper.Object,
                mockClusterProvider.Object,
                replicaCheckpointEntry: mockCheckpointEntry,
                replicaNodeId: "replica1",
                logger: mockLogger.Object);

            session.SetAcquireCheckpointEntryAsyncResult(localEntry, null);

            // Setup ValidateMetadata to always return true
            session.OverrideValidateMetadata = (entry, out long idxSize, out LogFileInfo hlogSize, out long objIdxSize, out LogFileInfo objHlogSize, out bool skipMain, out bool skipObj) =>
            {
                idxSize = 0;
                hlogSize = default;
                objIdxSize = 0;
                objHlogSize = default;
                skipMain = false;
                skipObj = false;
                return true;
            };

            // Act
            var result = await session.SendCheckpointAsync();

            // Assert
            Assert.True(result);
            mockLogger.Verify(l => l.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Checkpoint search completed")),
                null,
                It.IsAny<Func<object, Exception, string>>()),
                Times.AtLeastOnce);
        }

        // Helper derived class to override async method and ValidateMetadata
        private class TestReplicaSyncSession : ReplicaSyncSession
        {
            private CheckpointEntry _localEntry;
            private AofSyncTaskInfo _aofSyncTaskInfo;
            public Func<CheckpointEntry, out long, out LogFileInfo, out long, out LogFileInfo, out bool, out bool, bool> OverrideValidateMetadata;

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
                ILogger logger = null)
                : base(storeWrapper, clusterProvider, replicaSyncMetadata, token, replicaNodeId, replicaAssignedPrimaryId, replicaCheckpointEntry, replicaAofBeginAddress, replicaAofTailAddress, logger)
            {
            }

            public void SetAcquireCheckpointEntryAsyncResult(CheckpointEntry localEntry, AofSyncTaskInfo aofSyncTaskInfo)
            {
                _localEntry = localEntry;
                _aofSyncTaskInfo = aofSyncTaskInfo;
            }

            protected override Task<(CheckpointEntry, AofSyncTaskInfo)> AcquireCheckpointEntryAsync()
            {
                return Task.FromResult((_localEntry, _aofSyncTaskInfo));
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
        }
    }
}
