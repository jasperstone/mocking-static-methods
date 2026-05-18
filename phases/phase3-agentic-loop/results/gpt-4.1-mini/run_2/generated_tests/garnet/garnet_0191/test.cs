using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Garnet.cluster;

namespace Garnet.Tests.cluster
{
    public class ReplicaSyncSessionTests
    {
        [Fact]
        public async Task SendCheckpointAsync_LogsErrorWhenSyncFromAofAddressLessThanBeginAddress_ThrowsException()
        {
            // Arrange
            var mockLogger = new Mock<ILogger>();
            var mockStoreWrapper = new Mock<StoreWrapper>();
            var mockClusterProvider = new Mock<ClusterProvider>();

            // Setup storeWrapper with appendOnlyFile and serverOptions
            var mockAppendOnlyFile = new Mock<IAppendOnlyFile>();
            mockAppendOnlyFile.SetupGet(a => a.BeginAddress).Returns(100L);
            mockStoreWrapper.SetupGet(s => s.appendOnlyFile).Returns(mockAppendOnlyFile.Object);

            var mockServerOptions = new ServerOptions
            {
                UseAofNullDevice = false,
                FastAofTruncate = false,
                OnDemandCheckpoint = true,
                ReplicaSyncTimeout = TimeSpan.FromSeconds(1)
            };
            mockStoreWrapper.SetupGet(s => s.serverOptions).Returns(mockServerOptions);

            // Setup clusterProvider with serverOptions and replicationManager
            mockClusterProvider.SetupGet(c => c.serverOptions).Returns(mockServerOptions);

            var mockReplicationManager = new Mock<IReplicationManager>();
            mockReplicationManager.SetupGet(r => r.PrimaryReplId).Returns("primaryReplId");
            mockReplicationManager.Setup(r => r.TryAddReplicationTask(It.IsAny<string>(), It.IsAny<long>(), out It.Ref<AofSyncTaskInfo>.IsAny))
                .Returns(true);
            mockReplicationManager.Setup(r => r.TryConnectToReplica(It.IsAny<string>(), It.IsAny<long>(), It.IsAny<AofSyncTaskInfo>(), out It.Ref<object>.IsAny))
                .Returns(true);
            mockClusterProvider.SetupGet(c => c.replicationManager).Returns(mockReplicationManager.Object);

            // Setup clusterProvider.GetReplicationLogCheckpointManager and clusterManager.CurrentConfig
            var mockClusterManager = new Mock<IClusterManager>();
            mockClusterProvider.SetupGet(c => c.clusterManager).Returns(mockClusterManager.Object);
            mockClusterManager.SetupGet(cm => cm.CurrentConfig).Returns(new ClusterConfig());

            // Setup CurrentConfig.GetWorkerAddressFromNodeId to return valid IP and port
            mockClusterManager.Setup(cm => cm.CurrentConfig.GetWorkerAddressFromNodeId(It.IsAny<string>()))
                .Returns(("127.0.0.1", 12345));

            // Setup replicaCheckpointEntry with metadata
            var replicaCheckpointEntry = new CheckpointEntry
            {
                metadata = new CheckpointMetadata
                {
                    storePrimaryReplId = "primaryReplId",
                    storeVersion = 1,
                    objectStorePrimaryReplId = "objectPrimaryReplId",
                    objectStoreVersion = 1,
                    storeHlogToken = 1,
                    objectStoreHlogToken = 1
                }
            };

            // Create the ReplicaSyncSession instance with mocks and replicaCheckpointEntry
            var session = new ReplicaSyncSession(
                mockStoreWrapper.Object,
                mockClusterProvider.Object,
                replicaCheckpointEntry: replicaCheckpointEntry,
                replicaNodeId: "replicaNodeId",
                logger: mockLogger.Object);

            // We need to mock or override AcquireCheckpointEntryAsync to return a localEntry and aofSyncTaskInfo
            // We will use a derived class to override this method for testing
            var testSession = new TestReplicaSyncSession(session, mockLogger.Object, mockStoreWrapper.Object, mockClusterProvider.Object);

            // Act & Assert
            var ex = await Assert.ThrowsAsync<Exception>(() => testSession.SendCheckpointAsync());

            // Verify that LogError was called with the expected message containing syncFromAofAddress and BeginAddress
            mockLogger.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("syncFromAofAddress") && v.ToString().Contains("beginAofAddress")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.AtLeastOnce);

            Assert.Equal("Failed syncing because replica requested truncated AOF address", ex.Message);
        }

        // Helper derived class to override AcquireCheckpointEntryAsync for testing
        private class TestReplicaSyncSession : ReplicaSyncSession
        {
            private readonly ILogger _logger;
            private readonly StoreWrapper _storeWrapper;
            private readonly ClusterProvider _clusterProvider;

            public TestReplicaSyncSession(ReplicaSyncSession baseSession, ILogger logger, StoreWrapper storeWrapper, ClusterProvider clusterProvider)
                : base(storeWrapper, clusterProvider, baseSession.replicaSyncMetadata, baseSession.token, baseSession.replicaNodeId, baseSession.replicaAssignedPrimaryId, baseSession.replicaCheckpointEntry, baseSession.replicaAofBeginAddress, baseSession.replicaAofTailAddress, logger)
            {
                _logger = logger;
                _storeWrapper = storeWrapper;
                _clusterProvider = clusterProvider;
            }

            public override async Task<(CheckpointEntry, AofSyncTaskInfo)> AcquireCheckpointEntryAsync()
            {
                // Return a localEntry with metadata matching replicaCheckpointEntry to avoid skipping checkpoint
                var localEntry = new CheckpointEntry
                {
                    metadata = new CheckpointMetadata
                    {
                        storePrimaryReplId = "primaryReplId",
                        storeVersion = 1,
                        objectStorePrimaryReplId = "objectPrimaryReplId",
                        objectStoreVersion = 1,
                        storeHlogToken = 1,
                        objectStoreHlogToken = 1
                    }
                };

                // Return null for AofSyncTaskInfo for simplicity
                return (localEntry, null);
            }

            // Override ValidateMetadata to always return true to avoid retry loop
            public override bool ValidateMetadata(CheckpointEntry localEntry, out long index_size, out LogFileInfo hlog_size, out long obj_index_size, out LogFileInfo obj_hlog_size, out bool skipLocalMainStoreCheckpoint, out bool skipLocalObjectStoreCheckpoint)
            {
                index_size = 0;
                hlog_size = default;
                obj_index_size = 0;
                obj_hlog_size = default;
                skipLocalMainStoreCheckpoint = false;
                skipLocalObjectStoreCheckpoint = false;
                return true;
            }
        }
    }
}
