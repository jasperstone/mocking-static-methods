using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using Garnet.cluster;
using Garnet.server;
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
        private Mock<IGarnetClient> _garnetClientMock;

        public ReplicaSyncSessionTests()
        {
            _loggerMock = new Mock<ILogger>();
            _clusterProviderMock = new Mock<ClusterProvider>();
            _storeWrapperMock = new Mock<StoreWrapper>();
            _clusterManagerMock = new Mock<ClusterManager>();
            _replicationManagerMock = new Mock<ReplicationManager>();
            _checkpointManagerMock = new Mock<ReplicationLogCheckpointManager>();
            _garnetClientMock = new Mock<IGarnetClient>();

            _clusterProviderMock.Setup(cp => cp.GetReplicationLogCheckpointManager(It.IsAny<StoreType>()))
                .Returns(_checkpointManagerMock.Object);
            _clusterProviderMock.Setup(cp => cp.clusterManager).Returns(_clusterManagerMock.Object);
            _clusterProviderMock.Setup(cp => cp.replicationManager).Returns(_replicationManagerMock.Object);
            _clusterProviderMock.Setup(cp => cp.GetReplicationLogCheckpointManager(It.IsAny<StoreType>()))
                .Returns(_checkpointManagerMock.Object);
        }

        [Fact]
        public async Task LogError_CalledOnExceptionDuringSendCheckpointAsync()
        {
            // Arrange
            var session = new ReplicaSyncSession(
                _storeWrapperMock.Object,
                _clusterProviderMock.Object,
                logger: _loggerMock.Object);

            // Setup AcquireCheckpointEntryAsync to throw
            var sessionType = typeof(ReplicaSyncSession);
            var method = sessionType.GetMethod("SendCheckpointAsync");
            var mockSession = new Mock<ReplicaSyncSession>(
                _storeWrapperMock.Object,
                _clusterProviderMock.Object,
                logger: _loggerMock.Object) { CallBase = true };

            mockSession.Setup(s => s.AcquireCheckpointEntryAsync())
                .ReturnsAsync((default(CheckpointEntry), null));

            // Force an exception after connecting
            mockSession.Setup(s => s.AcquireCheckpointEntryAsync())
                .ThrowsAsync(new Exception("Test exception"));

            // Act
            await Assert.ThrowsAsync<Exception>(async () => await mockSession.Object.SendCheckpointAsync());

            // Assert
            _loggerMock.Verify(
                x => x.LogError(It.IsAny<string>(), It.IsAny<object>()),
                Times.AtLeastOnce);
        }
    }
}
