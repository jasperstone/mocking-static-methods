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
using System.Net;
using System.Net.Sockets;

namespace Garnet.Tests.cluster
{
    public class ReplicaSyncSessionTests
    {
        // We will test the logging of LogError on the condition where syncFromAofAddress < storeWrapper.appendOnlyFile.BeginAddress
        // and possibleAofDataLoss is false, which triggers the LogError call on ILogger.

        [Fact]
        public async Task SendCheckpointAsync_LogsError_WhenSyncFromAofAddressLessThanBeginAddressAndNoPossibleDataLoss()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();

            var appendOnlyFileMock = new Mock<AppendOnlyFile>();
            appendOnlyFileMock.SetupGet(a => a.BeginAddress).Returns(1000);

            var serverOptionsMock = new Mock<ServerOptions>();
            serverOptionsMock.SetupGet(s => s.UseAofNullDevice).Returns(false);
            serverOptionsMock.SetupGet(s => s.FastAofTruncate).Returns(true);
            serverOptionsMock.SetupGet(s => s.OnDemandCheckpoint).Returns(true);
            serverOptionsMock.SetupGet(s => s.ReplicaSyncTimeout).Returns(TimeSpan.FromSeconds(1));
            serverOptionsMock.SetupGet(s => s.TlsOptions).Returns((TlsOptions)null);

            var storeWrapperMock = new Mock<StoreWrapper>();
            storeWrapperMock.SetupGet(s => s.appendOnlyFile).Returns(appendOnlyFileMock.Object);
            storeWrapperMock.SetupGet(s => s.serverOptions).Returns(serverOptionsMock.Object);

            var replicationManagerMock = new Mock<ReplicationManager>();
            replicationManagerMock.SetupGet(r => r.PrimaryReplId).Returns("primaryReplId");
            replicationManagerMock.Setup(r => r.TryAddReplicationTask(It.IsAny<string>(), It.IsAny<long>(), out It.Ref<AofSyncTaskInfo>.IsAny)).Returns(true);
            replicationManagerMock.Setup(r => r.TryConnectToReplica(It.IsAny<string>(), It.IsAny<long>(), It.IsAny<AofSyncTaskInfo>(), out It.Ref<object>.IsAny)).Returns(true);

            var clusterConfigMock = new Mock<ClusterConfig>();
            clusterConfigMock.Setup(c => c.GetWorkerAddressFromNodeId(It.IsAny<string>())).Returns(("127.0.0.1", 12345));

            var clusterManagerMock = new Mock<ClusterManager>();
            clusterManagerMock.SetupGet(c => c.CurrentConfig).Returns(clusterConfigMock.Object);

            var clusterProviderMock = new Mock<ClusterProvider>();
            clusterProviderMock.SetupGet(c => c.replicationManager).Returns(replicationManagerMock.Object);
            clusterProviderMock.SetupGet(c => c.serverOptions).Returns(serverOptionsMock.Object);
            clusterProviderMock.SetupGet(c => c.clusterManager).Returns(clusterManagerMock.Object);
            clusterProviderMock.SetupGet(c => c.ClusterUsername).Returns("user");
            clusterProviderMock.SetupGet(c => c.ClusterPassword).Returns("pass");

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

            // Create a derived class to override AcquireCheckpointEntryAsync to return a localEntry and aofSyncTaskInfo
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

            var testSession = new TestReplicaSyncSession(
                storeWrapperMock.Object,
                clusterProviderMock.Object,
                replicaSyncMetadata: null,
                token: CancellationToken.None,
                replicaNodeId: "replica1",
                replicaAssignedPrimaryId: null,
                replicaCheckpointEntry: replicaCheckpointEntry,
                replicaAofBeginAddress: 0,
                replicaAofTailAddress: 0,
                logger: loggerMock.Object,
                localEntryToReturn: localEntry,
                syncFromAofAddressToReturn: 500 // less than BeginAddress 1000 to trigger error log
            );

            // Act & Assert
            var ex = await Assert.ThrowsAsync<Exception>(async () => await testSession.SendCheckpointAsync());

            // Verify that LogError was called with the expected message and parameters
            loggerMock.Verify(l => l.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("syncFromAofAddress: 500 < beginAofAddress: 1000")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);

            // Also verify that LogCheckpointEntry was called with LogLevel.Error and message "Requested replay address truncated"
            loggerMock.Verify(l => l.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Requested replay address truncated")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);

            Assert.Equal("Failed syncing because replica requested truncated AOF address", ex.Message);
        }

        private class TestReplicaSyncSession : ReplicaSyncSession
        {
            private readonly CheckpointEntry _localEntryToReturn;
            private readonly long _syncFromAofAddressToReturn;

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
                CheckpointEntry localEntryToReturn,
                long syncFromAofAddressToReturn)
                : base(storeWrapper, clusterProvider, replicaSyncMetadata, token, replicaNodeId, replicaAssignedPrimaryId, replicaCheckpointEntry, replicaAofBeginAddress, replicaAofTailAddress, logger)
            {
                _localEntryToReturn = localEntryToReturn;
                _syncFromAofAddressToReturn = syncFromAofAddressToReturn;
            }

            protected override async Task<(CheckpointEntry, AofSyncTaskInfo)> AcquireCheckpointEntryAsync()
            {
                return (_localEntryToReturn, new AofSyncTaskInfo(null, null, null, null, null, 0, null));
            }

            protected override async Task<string> ExecuteBeginReplicaRecoverAsync(
                GarnetClientSession gcs,
                bool skipLocalMainStoreCheckpoint,
                bool skipLocalObjectStoreCheckpoint,
                bool replayAOF,
                string primaryReplId,
                byte[] localEntryBytes,
                long beginAddress,
                long checkpointAofBeginAddress,
                CancellationToken token)
            {
                // Return the syncFromAofAddress as string to simulate the response
                return _syncFromAofAddressToReturn.ToString();
            }
        }
    }
}
