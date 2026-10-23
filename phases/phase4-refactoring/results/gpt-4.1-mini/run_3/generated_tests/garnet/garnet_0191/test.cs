using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Garnet.cluster;
using Garnet.common;
using Garnet.server;
using Xunit;

namespace Garnet.Tests
{
    public class ReplicaSyncSessionTests
    {
        [Fact]
        public async Task SendCheckpointAsync_LogsError_WhenSyncFromAofAddressLessThanBeginAddress()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();

            var appendOnlyFileMock = new Mock<IAppendOnlyFile>();
            appendOnlyFileMock.SetupGet(a => a.BeginAddress).Returns(100L);

            var serverOptionsMock = new Mock<IServerOptions>();
            serverOptionsMock.SetupGet(o => o.UseAofNullDevice).Returns(false);
            serverOptionsMock.SetupGet(o => o.FastAofTruncate).Returns(false);
            serverOptionsMock.SetupGet(o => o.OnDemandCheckpoint).Returns(true);
            serverOptionsMock.SetupGet(o => o.ReplicaSyncTimeout).Returns(TimeSpan.FromSeconds(1));

            var storeWrapperMock = new Mock<IStoreWrapper>();
            storeWrapperMock.SetupGet(s => s.appendOnlyFile).Returns(appendOnlyFileMock.Object);
            storeWrapperMock.SetupGet(s => s.serverOptions).Returns(serverOptionsMock.Object);

            var replicationManagerMock = new Mock<IReplicationManager>();
            replicationManagerMock.SetupGet(r => r.PrimaryReplId).Returns("primaryReplId");
            replicationManagerMock.Setup(r => r.TryAddReplicationTask(It.IsAny<string>(), It.IsAny<long>(), out It.Ref<AofSyncTaskInfo>.IsAny)).Returns(true);
            replicationManagerMock.Setup(r => r.TryConnectToReplica(It.IsAny<string>(), It.IsAny<long>(), It.IsAny<AofSyncTaskInfo>(), out It.Ref<object>.IsAny)).Returns(true);

            var clusterManagerMock = new Mock<IClusterManager>();
            clusterManagerMock.SetupGet(c => c.CurrentConfig).Returns(new ClusterConfigMock());

            var clusterProviderMock = new Mock<IClusterProvider>();
            clusterProviderMock.SetupGet(c => c.replicationManager).Returns(replicationManagerMock.Object);
            clusterProviderMock.SetupGet(c => c.serverOptions).Returns(serverOptionsMock.Object);
            clusterProviderMock.SetupGet(c => c.storeWrapper).Returns(storeWrapperMock.Object);
            clusterProviderMock.SetupGet(c => c.clusterManager).Returns(clusterManagerMock.Object);

            var replicaCheckpointEntry = new CheckpointEntryMock
            {
                metadata = new CheckpointMetadataMock
                {
                    storeVersion = 1,
                    objectStoreVersion = 1,
                    storePrimaryReplId = "primaryReplId",
                    objectStorePrimaryReplId = "primaryReplId"
                }
            };

            var session = new ReplicaSyncSession(
                storeWrapperMock.Object,
                clusterProviderMock.Object,
                replicaSyncMetadata: null,
                token: CancellationToken.None,
                replicaNodeId: "replica1",
                replicaAssignedPrimaryId: null,
                replicaCheckpointEntry: replicaCheckpointEntry,
                replicaAofBeginAddress: 0,
                replicaAofTailAddress: 0,
                logger: loggerMock.Object);

            // Act & Assert
            var ex = await Assert.ThrowsAsync<Exception>(() => session.SendCheckpointAsync());

            // Verify LogError was called with message containing syncFromAofAddress and beginAofAddress
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("syncFromAofAddress") && v.ToString().Contains("beginAofAddress")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.AtLeastOnce);
        }

        // Mocks for interfaces and classes used in the test
        private class ClusterConfigMock : IClusterConfig
        {
            public (string, int) GetWorkerAddressFromNodeId(string nodeId) => ("127.0.0.1", 1234);
        }

        private class CheckpointEntryMock : CheckpointEntry
        {
            public override CheckpointMetadata metadata { get; set; }
        }

        private class CheckpointMetadataMock : CheckpointMetadata
        {
            public override int storeVersion { get; set; }
            public override int objectStoreVersion { get; set; }
            public override string storePrimaryReplId { get; set; }
            public override string objectStorePrimaryReplId { get; set; }
        }

        // Interfaces to mock dependencies (simplified)
        private interface IAppendOnlyFile
        {
            long BeginAddress { get; }
        }

        private interface IServerOptions
        {
            bool UseAofNullDevice { get; }
            bool FastAofTruncate { get; }
            bool OnDemandCheckpoint { get; }
            TimeSpan ReplicaSyncTimeout { get; }
        }

        private interface IStoreWrapper
        {
            IAppendOnlyFile appendOnlyFile { get; }
            IServerOptions serverOptions { get; }
        }

        private interface IReplicationManager
        {
            string PrimaryReplId { get; }
            bool TryAddReplicationTask(string replicaNodeId, long syncFromAofAddress, out AofSyncTaskInfo aofSyncTaskInfo);
            bool TryConnectToReplica(string replicaNodeId, long syncFromAofAddress, AofSyncTaskInfo aofSyncTaskInfo, out object something);
        }

        private interface IClusterManager
        {
            IClusterConfig CurrentConfig { get; }
        }

        private interface IClusterConfig
        {
            (string, int) GetWorkerAddressFromNodeId(string nodeId);
        }

        private interface IClusterProvider
        {
            IReplicationManager replicationManager { get; }
            IServerOptions serverOptions { get; }
            IStoreWrapper storeWrapper { get; }
            IClusterManager clusterManager { get; }
        }

        private class AofSyncTaskInfo { }
        private class CheckpointEntry
        {
            public virtual CheckpointMetadata metadata { get; set; }
        }
        private class CheckpointMetadata
        {
            public virtual int storeVersion { get; set; }
            public virtual int objectStoreVersion { get; set; }
            public virtual string storePrimaryReplId { get; set; }
            public virtual string objectStorePrimaryReplId { get; set; }
        }
    }
}
