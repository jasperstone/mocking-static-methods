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
        private Mock<ILogger> _loggerMock;
        private Mock<ClusterProvider> _clusterProviderMock;
        private Mock<StoreWrapper> _storeWrapperMock;
        private Mock<ReplicationManager> _replicationManagerMock;
        private Mock<ClusterManager> _clusterManagerMock;
        private Mock<ReplicationLogCheckpointManager> _checkpointManagerMock;

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
            _clusterProviderMock.Setup(cp => cp.serverOptions).Returns(new ServerOptions());
            _clusterProviderMock.Setup(cp => cp.ClusterUsername).Returns("user");
            _clusterProviderMock.Setup(cp => cp.ClusterPassword).Returns("pass");
            // Setup TryGetLatestCheckpointEntryFromMemory to simulate success after first attempt
            bool firstCall = true;
            _checkpointManagerMock.Setup(m => m.TryGetLatestCheckpointEntryFromMemory(out It.Ref<CheckpointEntry>.IsAny))
                .Callback(new OutAction<CheckpointEntry>((out CheckpointEntry c) =>
                {
                    c = new CheckpointEntry();
                }))
                .Returns(() =>
                {
                    if (firstCall)
                    {
                        firstCall = false;
                        return true;
                    }
                    return false;
                });
        }

        [Fact]
        public async Task AcquireCheckpointEntryAsync_ShouldLogInformationEachIteration()
        {
            // Arrange
            var session = new ReplicaSyncSession(
                _storeWrapperMock.Object,
                _clusterProviderMock.Object,
                logger: _loggerMock.Object);

            // Act
            var result = await session.AcquireCheckpointEntryAsync();

            // Assert
            _loggerMock.Verify(
                x => x.LogInformation(It.Is<string>(s => s.Contains("AcquireCheckpointEntry iteration")), It.IsAny<object>()),
                Times.AtLeastOnce);
        }
    }
}
