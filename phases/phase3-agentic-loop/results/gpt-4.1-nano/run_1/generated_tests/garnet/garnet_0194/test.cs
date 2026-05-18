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
            _clusterProviderMock.Setup(cp => cp.serverOptions).Returns(new ServerOptions());
            _clusterProviderMock.Setup(cp => cp.ClusterUsername).Returns("user");
            _clusterProviderMock.Setup(cp => cp.ClusterPassword).Returns("pass");
            // Setup for clusterProvider.clusterManager.CurrentConfig
            var mockConfig = new Mock<ClusterConfig>();
            mockConfig.Setup(c => c.GetWorkerAddressFromNodeId(It.IsAny<string>()))
                .Returns(("127.0.0.1", 1234));
            _clusterProviderMock.Setup(cp => cp.clusterManager.CurrentConfig).Returns(mockConfig.Object);
        }

        [Fact]
        public async Task AcquireCheckpointEntryAsync_Should_LogInformationEachIteration()
        {
            // Arrange
            var session = new ReplicaSyncSession(
                _storeWrapperMock.Object,
                _clusterProviderMock.Object,
                logger: _loggerMock.Object);

            // Setup TryGetLatestCheckpointEntryFromMemory to simulate success after first attempt
            int callCount = 0;
            _checkpointManagerMock.Setup(m => m.TryGetLatestCheckpointEntryFromMemory(out It.Ref<CheckpointEntry>.IsAny))
                .Callback(new TryGetLatestCheckpointEntryFromMemoryCallback((out CheckpointEntry c) =>
                {
                    c = new CheckpointEntry();
                    callCount++;
                    return callCount > 1; // succeed after first call
                }))
                .Returns(true);

            // Act
            var result = await session.AcquireCheckpointEntryAsync();

            // Assert
            _loggerMock.Verify(
                x => x.LogInformation(It.Is<string>(s => s.Contains("AcquireCheckpointEntry iteration")), It.IsAny<object>()),
                Times.AtLeastOnce);
        }
    }

    // Delegate for mocking TryGetLatestCheckpointEntryFromMemory
    public delegate bool TryGetLatestCheckpointEntryFromMemoryCallback(out CheckpointEntry c);
}
