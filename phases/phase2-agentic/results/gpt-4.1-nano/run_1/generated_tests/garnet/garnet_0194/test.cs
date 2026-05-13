using System;
using System.Collections.Generic;
using System.Linq;
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
        private Mock<ILogger> _loggerMock;
        private Mock<ClusterProvider> _clusterProviderMock;
        private Mock<StoreWrapper> _storeWrapperMock;
        private Mock<ClusterManager> _clusterManagerMock;
        private Mock<ReplicationManager> _replicationManagerMock;
        private Mock<ReplicationLogCheckpointManager> _checkpointManagerMock;
        private Mock<ReplicationLogCheckpointManager> _checkpointManagerObjectMock;
        private Mock<ServerOptions> _serverOptionsMock;
        private Mock<ClusterOptions> _clusterOptionsMock;
        private Mock<ClusterConfig> _clusterConfigMock;
        private Mock<ReplicationManager> _replicationManager;
        private Mock<ClusterManager> _clusterManager;
        private Mock<ClusterProvider> _clusterProvider;
        private Mock<StoreWrapper> _storeWrapper;

        public ReplicaSyncSessionTests()
        {
            _loggerMock = new Mock<ILogger>();
            _clusterProviderMock = new Mock<ClusterProvider>();
            _storeWrapperMock = new Mock<StoreWrapper>();
            _clusterManagerMock = new Mock<ClusterManager>();
            _replicationManagerMock = new Mock<ReplicationManager>();
            _checkpointManagerMock = new Mock<ReplicationLogCheckpointManager>();
            _checkpointManagerObjectMock = new Mock<ReplicationLogCheckpointManager>();
            _serverOptionsMock = new Mock<ServerOptions>();
            _clusterOptionsMock = new Mock<ClusterOptions>();
            _clusterConfigMock = new Mock<ClusterConfig>();

            // Setup default behaviors
            _clusterProviderMock.Setup(cp => cp.GetReplicationLogCheckpointManager(It.IsAny<StoreType>()))
                .Returns(_checkpointManagerMock.Object);
            _clusterProviderMock.Setup(cp => cp.clusterManager).Returns(_clusterManagerMock.Object);
            _clusterProviderMock.Setup(cp => cp.replicationManager).Returns(_replicationManagerMock.Object);
            _clusterProviderMock.Setup(cp => cp.serverOptions).Returns(_serverOptionsMock.Object);
            _clusterProviderMock.Setup(cp => cp.ClusterUsername).Returns("user");
            _clusterProviderMock.Setup(cp => cp.ClusterPassword).Returns("pass");
            _clusterProviderMock.Setup(cp => cp.GetReplicationLogCheckpointManager(It.IsAny<StoreType>()))
                .Returns(_checkpointManagerMock.Object);
        }

        [Fact]
        public async Task SendCheckpointAsync_Should_LogError_When_ReplicaIdUnknown()
        {
            // Arrange
            var session = new ReplicaSyncSession(
                _storeWrapperMock.Object,
                _clusterProviderMock.Object,
                logger: _loggerMock.Object);

            // Setup cluster config to return null address
            var mockConfig = new Mock<ClusterConfig>();
            mockConfig.Setup(c => c.GetWorkerAddressFromNodeId(It.IsAny<string>()))
                .Returns((null, -1));
            _clusterProviderMock.Setup(cp => cp.clusterManager.CurrentConfig).Returns(mockConfig.Object);

            // Act
            var result = await session.SendCheckpointAsync();

            // Assert
            Assert.False(result);
            _loggerMock.Verify(l => l.LogError(It.IsAny<string>(), It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public async Task SendCheckpointAsync_Should_LogInformation_And_ReturnTrue_When_Successful()
        {
            // Arrange
            var mockConfig = new Mock<ClusterConfig>();
            mockConfig.Setup(c => c.GetWorkerAddressFromNodeId(It.IsAny<string>()))
                .Returns(("127.0.0.1", 1234));
            _clusterProviderMock.Setup(cp => cp.clusterManager.CurrentConfig).Returns(mockConfig.Object);

            var session = new ReplicaSyncSession(
                _storeWrapperMock.Object,
                _clusterProviderMock.Object,
                logger: _loggerMock.Object);

            // Setup AcquireCheckpointEntryAsync to return dummy data
            var checkpointEntry = new CheckpointEntry
            {
                metadata = new CheckpointMetadata
                {
                    storeVersion = 1,
                    objectStoreVersion = 1,
                    storePrimaryReplId = "id",
                    objectStorePrimaryReplId = "id"
                }
            };
            var aofSyncInfo = new AofSyncTaskInfo();

            // Use reflection to set private method behavior
            var method = typeof(ReplicaSyncSession).GetMethod("AcquireCheckpointEntryAsync", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var task = Task.FromResult<(CheckpointEntry, AofSyncTaskInfo)>((checkpointEntry, aofSyncInfo));
            var mockSession = new Mock<ReplicaSyncSession>(
                _storeWrapperMock.Object,
                _clusterProviderMock.Object,
                logger: _loggerMock.Object);
            mockSession.CallBase = true;
            mockSession.Setup(m => m.GetType().GetMethod("AcquireCheckpointEntryAsync", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance))
                .Returns(method);
            // Alternatively, we can create a derived class or use other techniques, but for simplicity, assume direct call

            // Act
            var result = await session.SendCheckpointAsync();

            // Assert
            Assert.True(result);
            _loggerMock.Verify(l => l.LogInformation(It.IsAny<string>(), It.IsAny<object[]>()), Times.AtLeastOnce);
        }

        [Fact]
        public void ValidateMetadata_Should_ReturnFalse_When_TryAcquireFails()
        {
            // Arrange
            var session = new ReplicaSyncSession(
                _storeWrapperMock.Object,
                _clusterProviderMock.Object,
                logger: _loggerMock.Object);

            var localEntry = new CheckpointEntry
            {
                metadata = new CheckpointMetadata
                {
                    storeHlogToken = 1,
                    storeVersion = 1,
                    storePrimaryReplId = "id",
                    objectStorePrimaryReplId = "id"
                }
            };

            // Setup TryAcquire to return false
            _clusterProviderMock.Setup(cp => cp.replicationManager.TryAcquireSettledMetadataForMainStore(It.IsAny<CheckpointEntry>(), out It.Ref<LogFileInfo>.IsAny))
                .Returns(false);

            // Act
            var result = session.ValidateMetadata(localEntry, out var indexSize, out var hlogSize, out var objIndexSize, out var objHlogSize, out var skipMain, out var skipObject);

            // Assert
            Assert.False(result);
        }
    }
}
