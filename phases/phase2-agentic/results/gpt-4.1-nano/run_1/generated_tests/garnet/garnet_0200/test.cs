using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using Garnet.cluster;
using Garnet.common;

namespace Garnet.Tests
{
    public class ReplicaSyncSessionTests
    {
        private readonly Mock<ILogger> _loggerMock;
        private readonly Mock<ClusterProvider> _clusterProviderMock;
        private readonly Mock<StoreWrapper> _storeWrapperMock;
        private readonly Mock<SyncMetadata> _syncMetadataMock;
        private readonly Mock<ReplicationManager> _replicationManagerMock;
        private readonly Mock<ClusterManager> _clusterManagerMock;
        private readonly Mock<ReplicationLogCheckpointManager> _checkpointManagerMock;
        private readonly Mock<GarnetClientSession> _garnetClientSessionMock;

        public ReplicaSyncSessionTests()
        {
            _loggerMock = new Mock<ILogger>();
            _clusterProviderMock = new Mock<ClusterProvider>();
            _storeWrapperMock = new Mock<StoreWrapper>();
            _syncMetadataMock = new Mock<SyncMetadata>();
            _replicationManagerMock = new Mock<ReplicationManager>();
            _clusterManagerMock = new Mock<ClusterManager>();
            _checkpointManagerMock = new Mock<ReplicationLogCheckpointManager>();
            _garnetClientSessionMock = new Mock<GarnetClientSession>();
        }

        [Fact]
        public async Task SendCheckpointAsync_Should_LogInformation_CallLogInformation()
        {
            // Arrange
            var session = new ReplicaSyncSession(
                _storeWrapperMock.Object,
                _clusterProviderMock.Object,
                logger: _loggerMock.Object);

            // Setup mocks
            var currentConfig = new ClusterConfig();
            _clusterProviderMock.Setup(c => c.clusterManager.CurrentConfig).Returns(currentConfig);
            var nodeId = "node1";
            var address = "127.0.0.1";
            var port = 1234;
            currentConfig.SetWorkerAddress(nodeId, address, port);
            _clusterProviderMock.Setup(c => c.GetReplicationLogCheckpointManager(It.IsAny<StoreType>()))
                .Returns(_checkpointManagerMock.Object);
            _clusterProviderMock.Setup(c => c.clusterManager).Returns(_clusterManagerMock.Object);
            _clusterProviderMock.Setup(c => c.ClusterUsername).Returns("user");
            _clusterProviderMock.Setup(c => c.ClusterPassword).Returns("pass");
            _clusterProviderMock.Setup(c => c.serverOptions).Returns(new ServerOptions());
            _clusterProviderMock.Setup(c => c.replicationManager).Returns(_replicationManagerMock.Object);
            _clusterProviderMock.Setup(c => c.GetReplicationLogCheckpointManager(It.IsAny<StoreType>()))
                .Returns(_checkpointManagerMock.Object);
            _clusterProviderMock.Setup(c => c.clusterManager.CurrentConfig).Returns(currentConfig);
            _clusterProviderMock.Setup(c => c.clusterManager.GetWorkerAddressFromNodeId(It.IsAny<string>()))
                .Returns((address, port));

            // Setup AcquireCheckpointEntryAsync to return dummy data
            var dummyEntry = new CheckpointEntry();
            var dummyAofSyncInfo = new AofSyncTaskInfo();
            var acquireTask = Task.FromResult<(CheckpointEntry, AofSyncTaskInfo)>((dummyEntry, dummyAofSyncInfo));
            var mockSession = new Mock<ReplicaSyncSession>(
                _storeWrapperMock.Object,
                _clusterProviderMock.Object,
                logger: _loggerMock.Object);
            mockSession.CallBase = true;
            mockSession.Setup(s => s.AcquireCheckpointEntryAsync()).Returns(acquireTask);

            // Setup ValidateMetadata to always return true
            mockSession.Setup(s => s.ValidateMetadata(It.IsAny<CheckpointEntry>(), out It.Ref<long>.IsAny, out It.Ref<LogFileInfo>.IsAny, out It.Ref<long>.IsAny, out It.Ref<LogFileInfo>.IsAny, out It.Ref<bool>.IsAny, out It.Ref<bool>.IsAny))
                .Returns(true);

            // Act
            var result = await mockSession.Object.SendCheckpointAsync();

            // Assert
            Assert.True(result);
            _loggerMock.Verify(l => l.LogInformation(It.Is<string>(s => s.Contains("requesting checkpoint")), It.IsAny<object[]>()), Times.AtLeastOnce);
            _loggerMock.Verify(l => l.LogInformation(It.Is<string>(s => s.Contains("Checkpoint search completed")), It.IsAny<object[]>()), Times.AtLeastOnce);
        }
    }
}
