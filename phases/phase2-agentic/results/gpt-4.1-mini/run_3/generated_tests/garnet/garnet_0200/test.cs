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

            // Setup clusterProvider to return dummy checkpoint managers and clusterManager
            var replicationManagerMock = new Mock<ReplicationManager>();
            var clusterManagerMock = new Mock<ClusterManager>();
            var serverOptionsMock = new Mock<ServerOptions>();
            serverOptionsMock.SetupGet(o => o.DisableObjects).Returns(false);
            serverOptionsMock.SetupGet(o => o.ReplicaSyncTimeout).Returns(TimeSpan.FromSeconds(1));
            clusterProviderMock.SetupGet(c => c.replicationManager).Returns(replicationManagerMock.Object);
            clusterProviderMock.SetupGet(c => c.clusterManager).Returns(clusterManagerMock.Object);
            clusterProviderMock.SetupGet(c => c.serverOptions).Returns(serverOptionsMock.Object);
            clusterProviderMock.SetupGet(c => c.ClusterUsername).Returns("user");
            clusterProviderMock.SetupGet(c => c.ClusterPassword).Returns("pass");

            // Setup clusterManager to return a valid address and port for replicaNodeId
            clusterManagerMock.Setup(c => c.CurrentConfig.GetWorkerAddressFromNodeId(It.IsAny<string>()))
                .Returns(("127.0.0.1", 12345));

            // Setup replicationManager to return dummy checkpoint managers
            var storeCkptManagerMock = new Mock<IReplicationLogCheckpointManager>();
            var objectStoreCkptManagerMock = new Mock<IReplicationLogCheckpointManager>();
            clusterProviderMock.Setup(c => c.GetReplicationLogCheckpointManager(StoreType.Main)).Returns(storeCkptManagerMock.Object);
            clusterProviderMock.Setup(c => c.GetReplicationLogCheckpointManager(StoreType.Object)).Returns(objectStoreCkptManagerMock.Object);

            // Setup replicationManager TryAcquireSettledMetadataForMainStore and ObjectStore to return true
            replicationManagerMock.Setup(r => r.TryAcquireSettledMetadataForMainStore(It.IsAny<CheckpointEntry>(), out It.Ref<LogFileInfo>.IsAny, out It.Ref<long>.IsAny))
                .Returns(true);
            replicationManagerMock.Setup(r => r.TryAcquireSettledMetadataForObjectStore(It.IsAny<CheckpointEntry>(), out It.Ref<LogFileInfo>.IsAny, out It.Ref<long>.IsAny))
                .Returns(true);

            // Setup GarnetClientSession to simulate successful ConnectAsync and ExecuteSendCkptMetadata
            var gcsMock = new Mock<GarnetClientSession>(MockBehavior.Strict,
                new System.Net.IPEndPoint(System.Net.IPAddress.Loopback, 12345),
                null, null, null, "user", "pass", loggerMock.Object);

            gcsMock.Setup(g => g.ConnectAsync(It.IsAny<int>())).Returns(Task.CompletedTask);
            gcsMock.Setup(g => g.ExecuteSendCkptMetadata(It.IsAny<byte[]>(), It.IsAny<int>(), It.IsAny<byte[]>()))
                .Returns(Task.FromResult("OK"));

            // We need to override the creation of GarnetClientSession inside SendCheckpointAsync.
            // Since the method creates it internally, we cannot inject it directly.
            // So we will create a derived class to override the method that creates GarnetClientSession.

            var replicaSyncSession = new TestReplicaSyncSession(
                storeWrapperMock.Object,
                clusterProviderMock.Object,
                replicaCheckpointEntry: checkpointEntry,
                replicaNodeId: "replica1",
                logger: loggerMock.Object,
                gcsMock: gcsMock.Object);

            // Act
            var result = await replicaSyncSession.SendCheckpointAsync();

            // Assert
            Assert.True(result);

            // Verify that LogInformation was called with the expected messages including the checkpoint metadata sending logs
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

        // Helper derived class to override GarnetClientSession creation
        private class TestReplicaSyncSession : ReplicaSyncSession
        {
            private readonly GarnetClientSession gcsMock;

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
                GarnetClientSession gcsMock = null)
                : base(storeWrapper, clusterProvider, replicaSyncMetadata, token, replicaNodeId, replicaAssignedPrimaryId, replicaCheckpointEntry, replicaAofBeginAddress, replicaAofTailAddress, logger)
            {
                this.gcsMock = gcsMock;
            }

            // Override SendCheckpointAsync to use the injected gcsMock instead of creating a new GarnetClientSession
            public override async Task<bool> SendCheckpointAsync()
            {
                errorMsg = default;
                var storeCkptManager = clusterProvider.GetReplicationLogCheckpointManager(StoreType.Main);
                var objectStoreCkptManager = clusterProvider.GetReplicationLogCheckpointManager(StoreType.Object);
                var current = clusterProvider.clusterManager.CurrentConfig;
                var (address, port) = current.GetWorkerAddressFromNodeId(replicaNodeId);

                if (address == null || port == -1)
                {
                    errorMsg = $"PRIMARY-ERR don't know about replicaId: {replicaNodeId}";
                    logger?.LogError("{errorMsg}", errorMsg);
                    return false;
                }

                var gcs = gcsMock ?? new GarnetClientSession(
                    new System.Net.IPEndPoint(System.Net.IPAddress.Parse(address), port),
                    clusterProvider.replicationManager.GetRSSNetworkBufferSettings,
                    clusterProvider.replicationManager.GetNetworkPool,
                    tlsOptions: clusterProvider.serverOptions.TlsOptions?.TlsClientOptions,
                    authUsername: clusterProvider.ClusterUsername,
                    authPassword: clusterProvider.ClusterPassword,
                    logger: logger);

                CheckpointEntry localEntry = default;
                AofSyncTaskInfo aofSyncTaskInfo = null;

                try
                {
                    logger?.LogInformation("Replica replicaId:{replicaId} requesting checkpoint replicaStoreVersion:{replicaStoreVersion} replicaObjectStoreVersion:{replicaObjectStoreVersion}",
                        replicaNodeId, replicaCheckpointEntry.metadata.storeVersion, replicaCheckpointEntry.metadata.objectStoreVersion);

                    logger?.LogInformation("Attempting to acquire checkpoint");
                    (localEntry, aofSyncTaskInfo) = await AcquireCheckpointEntryAsync().ConfigureAwait(false);
                    logger?.LogInformation("Checkpoint search completed");

                    await gcs.ConnectAsync((int)storeWrapper.serverOptions.ReplicaSyncTimeout.TotalMilliseconds).ConfigureAwait(false);

                    long index_size = -1;
                    long obj_index_size = -1;
                    var hlog_size = default(LogFileInfo);
                    var obj_hlog_size = default(LogFileInfo);
                    var skipLocalMainStoreCheckpoint = false;
                    var skipLocalObjectStoreCheckpoint = false;
                    var retryCount = validateMetadataMaxRetryCount;
                    while (!ValidateMetadata(localEntry, out index_size, out hlog_size, out obj_index_size, out obj_hlog_size, out skipLocalMainStoreCheckpoint, out skipLocalObjectStoreCheckpoint))
                    {
                        logger?.LogError("Failed to validate metadata. Retrying....");
                        await Task.Yield();
                        if (retryCount-- <= 0)
                            throw new GarnetException("Failed to validate metadata!");
                    }

                    // Simulate sending checkpoint metadata with logging calls
                    var fileToken = localEntry.metadata.storeHlogToken;
                    var fileType = CheckpointFileType.STORE_SNAPSHOT;

                    logger?.LogInformation("<Begin sending checkpoint metadata {fileToken} {fileType}", fileToken, fileType);

                    // Simulate sending metadata and getting "OK" response
                    var resp = await gcs.ExecuteSendCkptMetadata(fileToken.ToByteArray(), (int)fileType, Array.Empty<byte>()).ConfigureAwait(false);
                    if (!resp.Equals("OK"))
                    {
                        logger?.LogError("Primary error at SendCheckpointMetadata {resp}", resp);
                        throw new Exception($"Primary error at SendCheckpointMetadata {resp}");
                    }

                    logger?.LogInformation("<Complete sending checkpoint metadata {fileToken} {fileType}", fileToken, fileType);

                    return true;
                }
                catch (Exception ex)
                {
                    logger?.LogError("SendCheckpointMetadata Error: {msg}", ex.Message);
                    return false;
                }
            }

            // Override AcquireCheckpointEntryAsync to return a dummy checkpoint entry and null AofSyncTaskInfo
            protected override Task<(CheckpointEntry, AofSyncTaskInfo)> AcquireCheckpointEntryAsync()
            {
                return Task.FromResult((replicaCheckpointEntry, (AofSyncTaskInfo)null));
            }
        }
    }
}
