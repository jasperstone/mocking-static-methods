using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Garnet.cluster.Tests
{
    public class FailoverSessionTests
    {
        // We will test the TakeOverAsPrimaryAsync method's logging behavior when BeginRecovery returns false.
        // This triggers the LogWarning call on the logger with the message about not acquiring recovery lock.

        private class TestFailoverSession : FailoverSession
        {
            public TestFailoverSession(
                Mock<ILogger> loggerMock,
                Mock<IClusterProvider> clusterProviderMock)
            {
                this.logger = loggerMock.Object;
                this.clusterProvider = clusterProviderMock.Object;
            }

            // Expose the private method for testing via a public wrapper
            public Task<bool> TakeOverAsPrimaryAsyncPublic() => TakeOverAsPrimaryAsync();
        }

        // Interfaces to mock dependencies used in FailoverSession
        public interface IClusterProvider
        {
            IReplicationManager replicationManager { get; }
            IClusterManager clusterManager { get; }
            IStoreWrapper storeWrapper { get; }
            Task BumpAndWaitForEpochTransitionAsync();
        }

        public interface IReplicationManager
        {
            bool BeginRecovery(RecoveryStatus status, bool upgradeLock);
            void EndRecovery(RecoveryStatus status, bool downgradeLock);
            void TryUpdateForFailover();
            void ResetReplayIterator();
            bool InitializeCheckpointStore();
        }

        public interface IClusterManager
        {
            bool TryTakeOverForPrimary();
            object CurrentConfig { get; }
        }

        public interface IStoreWrapper
        {
            void StartPrimaryTasks();
        }

        public enum RecoveryStatus
        {
            ClusterFailover,
            NoRecovery
        }

        [Fact]
        public async Task TakeOverAsPrimaryAsync_LogsWarning_WhenBeginRecoveryFails()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var replicationManagerMock = new Mock<IReplicationManager>();
            var clusterManagerMock = new Mock<IClusterManager>();
            var storeWrapperMock = new Mock<IStoreWrapper>();
            var clusterProviderMock = new Mock<IClusterProvider>();

            // Setup BeginRecovery to return false to trigger the warning log
            replicationManagerMock.Setup(r => r.BeginRecovery(RecoveryStatus.ClusterFailover, false)).Returns(false);

            clusterProviderMock.SetupGet(c => c.replicationManager).Returns(replicationManagerMock.Object);
            clusterProviderMock.SetupGet(c => c.clusterManager).Returns(clusterManagerMock.Object);
            clusterProviderMock.SetupGet(c => c.storeWrapper).Returns(storeWrapperMock.Object);
            clusterProviderMock.Setup(c => c.BumpAndWaitForEpochTransitionAsync()).Returns(Task.CompletedTask);

            var failoverSession = new TestFailoverSession(loggerMock, clusterProviderMock);

            // Act
            var result = await failoverSession.TakeOverAsPrimaryAsyncPublic();

            // Assert
            Assert.False(result);

            // Verify that LogWarning was called with the expected message format and argument
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("TakeOverAsPrimaryAsync:")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }
    }
}
