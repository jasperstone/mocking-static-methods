using System;
using System.Net;
using System.Threading;
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
            var clusterManagerMock = new Mock<ClusterManager>();
            var currentConfigMock = new Mock<CurrentConfig>();
            var replicationManagerMock = new Mock<ReplicationManager>();
            var storeWrapperMock = new Mock<StoreWrapper>();
            var serverOptionsMock = new Mock<ServerOptions>();

            // Setup replicaNodeId and checkpointEntry metadata
            string replicaNodeId = "replica1";
            var checkpointEntry = new CheckpointEntry
            {
                metadata = new Metadata
                {
                    storeVersion = 1,
                    objectStoreVersion = 1,
                    storePrimaryReplId = "primary1",
                    objectStorePrimaryReplId = "primary1",
                    storeHlogToken = new Token(),
                    storeIndexToken = new Token(),
                    objectStoreHlogToken = new Token(),
                    objectStoreIndexToken = new Token()
                }
            };

            // Setup clusterManager to return currentConfig
            clusterProviderMock.SetupGet(cp => cp.clusterManager).Returns(clusterManagerMock.Object);
            clusterManagerMock.SetupGet(cm => cm.CurrentConfig).Returns(currentConfigMock.Object);

            // Setup currentConfig to return address and port for replicaNodeId
            currentConfigMock.Setup(cc => cc.GetWorkerAddressFromNodeId(replicaNodeId))
                .Returns(("127.0.0.1", 12345));

            // Setup replicationManager
            clusterProviderMock.SetupGet(cp => cp.replicationManager).Returns(replicationManagerMock.Object);

            // Setup serverOptions
            var serverOptions = new ServerOptions
            {
                ReplicaSyncTimeout = TimeSpan.FromSeconds(1),
                TlsOptions = null,
                EnableStorageTier = false,
                DisableObjects = false
            };
            clusterProviderMock.SetupGet(cp => cp.serverOptions).Returns(serverOptions);

            // Setup ClusterUsername and ClusterPassword
            clusterProviderMock.SetupGet(cp => cp.ClusterUsername).Returns("user");
            clusterProviderMock.SetupGet(cp => cp.ClusterPassword).Returns("pass");

            // Setup storeWrapper serverOptions
            storeWrapperMock.SetupGet(sw => sw.serverOptions).Returns(serverOptions);

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
                    storeIndexToken = new Token(),
                    objectStoreHlogToken = new Token(),
                    objectStoreIndexToken = new Token()
                }
            };

            // Create a derived class to override AcquireCheckpointEntryAsync and ValidateMetadata
            var session = new TestReplicaSyncSession(
                storeWrapperMock.Object,
                clusterProviderMock.Object,
                replicaSyncMetadata: null,
                token: CancellationToken.None,
                replicaNodeId: replicaNodeId,
                replicaAssignedPrimaryId: null,
                replicaCheckpointEntry: checkpointEntry,
                replicaAofBeginAddress: 0,
                replicaAofTailAddress: 0,
                logger: loggerMock.Object)
            {
                AcquireCheckpointEntryAsyncResult = (localEntry, null),
                ValidateMetadataResult = true
            };

            // Act
            var result = await session.SendCheckpointAsync();

            // Assert
            Assert.True(result);
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
            public (CheckpointEntry, AofSyncTaskInfo) AcquireCheckpointEntryAsyncResult { get; set; }
            public bool ValidateMetadataResult { get; set; }

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

            protected override Task<(CheckpointEntry, AofSyncTaskInfo)> AcquireCheckpointEntryAsync()
            {
                return Task.FromResult(AcquireCheckpointEntryAsyncResult);
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
                return ValidateMetadataResult;
            }
        }
    }

    // Minimal stubs for types used in the test
    public class ClusterProvider
    {
        public virtual ClusterManager clusterManager { get; }
        public virtual ReplicationManager replicationManager { get; }
        public virtual ServerOptions serverOptions { get; }
        public virtual string ClusterUsername { get; }
        public virtual string ClusterPassword { get; }
        public virtual ReplicationLogCheckpointManager GetReplicationLogCheckpointManager(StoreType storeType) => null;
    }

    public class ClusterManager
    {
        public virtual CurrentConfig CurrentConfig { get; }
    }

    public class CurrentConfig
    {
        public virtual (string, int) GetWorkerAddressFromNodeId(string nodeId) => (null, -1);
    }

    public class ReplicationManager
    {
        public virtual object GetRSSNetworkBufferSettings { get; }
        public virtual object GetNetworkPool { get; }
        public virtual bool TryAcquireSettledMetadataForMainStore(CheckpointEntry entry, out LogFileInfo hlogSize, out long indexSize)
        {
            hlogSize = default;
            indexSize = 0;
            return true;
        }
        public virtual bool TryAcquireSettledMetadataForObjectStore(CheckpointEntry entry, out LogFileInfo hlogSize, out long indexSize)
        {
            hlogSize = default;
            indexSize = 0;
            return true;
        }
    }

    public class StoreWrapper
    {
        public virtual ServerOptions serverOptions { get; }
    }

    public class ServerOptions
    {
        public TimeSpan ReplicaSyncTimeout { get; set; }
        public TlsOptions TlsOptions { get; set; }
        public bool EnableStorageTier { get; set; }
        public bool DisableObjects { get; set; }
    }

    public class TlsOptions
    {
        public object TlsClientOptions { get; set; }
    }

    public class CheckpointEntry
    {
        public Metadata metadata { get; set; }
    }

    public class Metadata
    {
        public int storeVersion { get; set; }
        public int objectStoreVersion { get; set; }
        public string storePrimaryReplId { get; set; }
        public string objectStorePrimaryReplId { get; set; }
        public Token storeHlogToken { get; set; }
        public Token storeIndexToken { get; set; }
        public Token objectStoreHlogToken { get; set; }
        public Token objectStoreIndexToken { get; set; }
    }

    public class Token { }

    public class LogFileInfo { }

    public class AofSyncTaskInfo { }
}
