using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using Garnet.cluster;
using Garnet.cluster.Server.Replication;

namespace Garnet.cluster.Tests
{
    public class ReplicationManagerTests
    {
        [Fact]
        public void BeginRecovery_WhenCurrentRecoveryStatusIsNotNoRecovery_LogsError()
        {
            // Arrange
            var mockLogger = new Mock<ILogger>();
            var mockClusterProvider = new Mock<ClusterProvider>();
            var mockStoreWrapper = new Mock<StoreWrapper>();
            var mockRecoverLock = new Mock<SingleWriterMultiReaderLock>();

            var replicationManager = new ReplicationManager(mockClusterProvider.Object, mockLogger.Object);
            replicationManager.currentRecoveryStatus = RecoveryStatus.InitializeRecover;

            // Act
            var result = replicationManager.BeginRecovery(RecoveryStatus.ReadRole, false);

            // Assert
            mockLogger.Verify(
                x => x.Log(
                    It.IsAny<LogLevel>(),
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Error background recovering task has not completed")),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
                Times.Once);
            Assert.False(result);
        }
    }
}
