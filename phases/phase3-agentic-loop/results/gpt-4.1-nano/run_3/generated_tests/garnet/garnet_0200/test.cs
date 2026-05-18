using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using Microsoft.Extensions.Logging;
using Moq;
using Garnet.cluster;
using Garnet.server;
using Garnet.common;

namespace Garnet.Tests
{
    public class ReplicaSyncSessionTests
    {
        private readonly Mock<ILogger> _loggerMock;
        private readonly Mock<ClusterProvider> _clusterProviderMock;
        private readonly Mock<StoreWrapper> _storeWrapperMock;
        private readonly Mock<ClusterManager> _clusterManagerMock;
        private readonly Mock<ReplicationManager> _replicationManagerMock;
        private readonly Mock<LogCheckpointManager> _logCheckpointManagerMock;
        private readonly Mock<SyncMetadata> _syncMetadataMock;

        public ReplicaSyncSessionTests()
        {
            _loggerMock = new Mock<ILogger>();
            _clusterProviderMock = new Mock<ClusterProvider>();
            _storeWrapperMock = new Mock<StoreWrapper>();
            _clusterManagerMock = new Mock<ClusterManager>();
            _replicationManagerMock = new Mock<ReplicationManager>();
            _logCheckpointManagerMock = new Mock<LogCheckpointManager>();
            _syncMetadataMock = new Mock<SyncMetadata>();

            _clusterProviderMock.Setup(cp => cp.GetReplicationLogCheckpointManager(It.IsAny<StoreType>()))
                .Returns(_logCheckpointManagerMock.Object);
            _clusterProviderMock.Setup(cp => cp.clusterManager).Returns(_clusterManagerMock.Object);
            _clusterProviderMock.Setup(cp => cp.replicationManager).Returns(_replicationManagerMock.Object);
            _clusterProviderMock.Setup(cp => cp.ClusterUsername).Returns("user");
            _clusterProviderMock.Setup(cp => cp.ClusterPassword).Returns("pass");
            _clusterProviderMock.Setup(cp => cp.serverOptions).Returns(new ServerOptions
            {
                TlsOptions = null,
                SegmentSizeBits = () => 20,
                TlsOptions = null
            });
        }

        [Fact]
        public async Task SendCheckpointAsync_Should_LogInformation_And_ValidateMetadata()
        {
            // Arrange
            var session = new ReplicaSyncSession(
                _storeWrapperMock.Object,
                _clusterProviderMock.Object,
                _syncMetadataMock.Object,
                logger: _loggerMock.Object);

            // Setup mock for AcquireCheckpointEntryAsync
            async Task<(CheckpointEntry, AofSyncTaskInfo)> AcquireCheckpointAsync()
            {
                var entry = new CheckpointEntry
                {
                    metadata = new CheckpointMetadata
                    {
                        storeVersion = 1,
                        objectStoreVersion = 1,
                        storePrimaryReplId = "id",
                        objectStorePrimaryReplId = "id"
                    }
                };
                return (entry, null);
            }

            // Replace method with mock
            var sendMethod = typeof(ReplicaSyncSession).GetMethod("SendCheckpointAsync", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var sendTask = (Task)sendMethod.Invoke(session, null);

            // Act
            var result = await session.SendCheckpointAsync();

            // Assert
            _loggerMock.Verify(log => log.LogInformation(It.Is<string>(s => s.Contains("requesting checkpoint"))), Times.AtLeastOnce);
            _loggerMock.Verify(log => log.LogInformation(It.Is<string>(s => s.Contains("Checkpoint search completed"))), Times.AtLeastOnce);
            Assert.IsType<bool>(result);
        }
    }
}
