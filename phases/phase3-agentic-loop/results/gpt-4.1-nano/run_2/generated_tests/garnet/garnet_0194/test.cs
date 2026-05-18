using System;
using System.Threading.Tasks;
using Xunit;
using Microsoft.Extensions.Logging;
using Moq;
using Garnet.cluster;
using Garnet.common;

namespace Garnet.Tests
{
    public class ReplicaSyncSessionTests
    {
        private readonly Mock<ILogger> _loggerMock;
        private readonly Mock<ClusterProvider> _clusterProviderMock;
        private readonly Mock<StoreWrapper> _storeWrapperMock;
        private readonly Mock<ReplicationManager> _replicationManagerMock;
        private readonly Mock<ClusterManager> _clusterManagerMock;
        private readonly Mock<ReplicationLogCheckpointManager> _checkpointManagerMock;

        public ReplicaSyncSessionTests()
        {
            _loggerMock = new Mock<ILogger>();
            _clusterProviderMock = new Mock<ClusterProvider>();
            _storeWrapperMock = new Mock<StoreWrapper>();
            _replicationManagerMock = new Mock<ReplicationManager>();
            _clusterManagerMock = new Mock<ClusterManager>();
            _checkpointManagerMock = new Mock<ReplicationLogCheckpointManager>();

            _clusterProviderMock.Setup(cp => cp.replicationManager).Returns(_replicationManagerMock.Object);
            _clusterProviderMock.Setup(cp => cp.clusterManager).Returns(_clusterManagerMock.Object);
            _clusterProviderMock.Setup(cp => cp.GetReplicationLogCheckpointManager(It.IsAny<StoreType>()))
                .Returns(_checkpointManagerMock.Object);
        }

        [Fact]
        public async Task AcquireCheckpointEntryAsync_ShouldLogInformationEachIteration()
        {
            // Arrange
            var session = new ReplicaSyncSession(
                _storeWrapperMock.Object,
                _clusterProviderMock.Object,
                logger: _loggerMock.Object);

            // Setup TryGetLatestCheckpointEntryFromMemory to simulate success after first attempt
            var callCount = 0;
            _checkpointManagerMock.Setup(m => m.TryGetLatestCheckpointEntryFromMemory(out It.Ref<CheckpointEntry>.IsAny))
                .Returns(() =>
                {
                    callCount++;
                    if (callCount == 1)
                        return false; // Fail first attempt
                    else
                        return true; // Succeed on second attempt
                });

            // Act
            var result = await session.AcquireCheckpointEntryAsync();

            // Assert
            Assert.IsType<(CheckpointEntry, AofSyncTaskInfo)>(result);
            _loggerMock.Verify(
                x => x.LogInformation(It.Is<string>(s => s.Contains("AcquireCheckpointEntry iteration")), It.IsAny<object>()),
                Times.AtLeastOnce);
        }
    }
}
