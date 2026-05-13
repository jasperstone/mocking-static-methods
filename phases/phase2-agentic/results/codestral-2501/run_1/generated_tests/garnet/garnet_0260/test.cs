using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using Garnet.cluster;
using Garnet.cluster.Server.Replication;

namespace Garnet.cluster.Tests
{
    public class ReplicationManagerTests
    {
        private readonly Mock<ILogger<ReplicationManager>> _loggerMock;
        private readonly Mock<ClusterProvider> _clusterProviderMock;
        private readonly Mock<StoreWrapper> _storeWrapperMock;
        private readonly ReplicationManager _replicationManager;

        public ReplicationManagerTests()
        {
            _loggerMock = new Mock<ILogger<ReplicationManager>>();
            _clusterProviderMock = new Mock<ClusterProvider>();
            _storeWrapperMock = new Mock<StoreWrapper>();
            _replicationManager = new ReplicationManager(_clusterProviderMock.Object, _loggerMock.Object);
        }

        [Fact]
        public void BeginRecovery_LogsError_WhenCurrentRecoveryStatusIsNotNoRecovery()
        {
            // Arrange
            _replicationManager.currentRecoveryStatus = RecoveryStatus.InitializeRecover;
            var nextRecoveryStatus = RecoveryStatus.ClusterReplicate;

            // Act
            var result = _replicationManager.BeginRecovery(nextRecoveryStatus, false);

            // Assert
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Error background recovering task has not completed")),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
                Times.Once);

            Assert.False(result);
        }

        [Fact]
        public void BeginRecovery_LogsError_WhenCannotAcquireCheckpointLock()
        {
            // Arrange
            _replicationManager.currentRecoveryStatus = RecoveryStatus.NoRecovery;
            _clusterProviderMock.Setup(x => x.storeWrapper).Returns(_storeWrapperMock.Object);
            _storeWrapperMock.Setup(x => x.TryPauseCheckpoints()).Returns(false);
            var nextRecoveryStatus = RecoveryStatus.ClusterReplicate;

            // Act
            var result = _replicationManager.BeginRecovery(nextRecoveryStatus, false);

            // Assert
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Error could not acquire checkpoint lock")),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
                Times.Once);

            Assert.False(result);
        }

        [Fact]
        public void BeginRecovery_LogsError_WhenCannotAcquireRecoverLock()
        {
            // Arrange
            _replicationManager.currentRecoveryStatus = RecoveryStatus.NoRecovery;
            _clusterProviderMock.Setup(x => x.storeWrapper).Returns(_storeWrapperMock.Object);
            _storeWrapperMock.Setup(x => x.TryPauseCheckpoints()).Returns(true);
            _storeWrapperMock.Setup(x => x.ResumeCheckpoints());
            var nextRecoveryStatus = RecoveryStatus.ClusterReplicate;

            // Act
            var result = _replicationManager.BeginRecovery(nextRecoveryStatus, false);

            // Assert
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Error could not acquire recover lock")),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
                Times.Once);

            Assert.False(result);
        }

        [Fact]
        public void BeginRecovery_LogsTrace_WhenSuccessfullyAcquiresRecoverLock()
        {
            // Arrange
            _replicationManager.currentRecoveryStatus = RecoveryStatus.NoRecovery;
            _clusterProviderMock.Setup(x => x.storeWrapper).Returns(_storeWrapperMock.Object);
            _storeWrapperMock.Setup(x => x.TryPauseCheckpoints()).Returns(true);
            _storeWrapperMock.Setup(x => x.ResumeCheckpoints());
            var nextRecoveryStatus = RecoveryStatus.ClusterReplicate;

            // Act
            var result = _replicationManager.BeginRecovery(nextRecoveryStatus, false);

            // Assert
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Trace,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Success recover lock")),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
                Times.Once);

            Assert.True(result);
        }
    }
}
