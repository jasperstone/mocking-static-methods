using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Garnet.cluster.Tests
{
    public class FailoverSessionTests
    {
        private class TestFailoverSession : FailoverSession
        {
            public TestFailoverSession(ILogger logger, IClusterProvider clusterProvider)
            {
                this.logger = logger;
                this.clusterProvider = clusterProvider;
            }

            public new async Task<bool> TakeOverAsPrimaryAsync()
            {
                return await base.TakeOverAsPrimaryAsync();
            }
        }

        private interface IClusterProvider
        {
            IReplicationManager replicationManager { get; }
            IClusterManager clusterManager { get; }
            IStoreWrapper storeWrapper { get; }
            Task BumpAndWaitForEpochTransitionAsync();
        }

        private interface IReplicationManager
        {
            bool BeginRecovery(RecoveryStatus status, bool upgradeLock);
            void EndRecovery(RecoveryStatus status, bool downgradeLock);
            void TryUpdateForFailover();
            void ResetReplayIterator();
            bool InitializeCheckpointStore();
        }

        private interface IClusterManager
        {
            bool TryTakeOverForPrimary();
        }

        private interface IStoreWrapper
        {
            void StartPrimaryTasks();
        }

        private enum RecoveryStatus
        {
            ClusterFailover,
            NoRecovery
        }

        [Fact]
        public async Task TakeOverAsPrimaryAsync_BeginRecoveryFails_LogsWarningAndReturnsFalse()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var replicationManagerMock = new Mock<IReplicationManager>();
            var clusterManagerMock = new Mock<IClusterManager>();
            var storeWrapperMock = new Mock<IStoreWrapper>();
            var clusterProviderMock = new Mock<IClusterProvider>();

            clusterProviderMock.SetupGet(c => c.replicationManager).Returns(replicationManagerMock.Object);
            clusterProviderMock.SetupGet(c => c.clusterManager).Returns(clusterManagerMock.Object);
            clusterProviderMock.SetupGet(c => c.storeWrapper).Returns(storeWrapperMock.Object);
            clusterProviderMock.Setup(c => c.BumpAndWaitForEpochTransitionAsync()).Returns(Task.CompletedTask);

            replicationManagerMock.Setup(r => r.BeginRecovery(RecoveryStatus.ClusterFailover, false)).Returns(false);

            var session = new TestFailoverSession(loggerMock.Object, clusterProviderMock.Object);

            // Act
            var result = await session.TakeOverAsPrimaryAsync();

            // Assert
            Assert.False(result);
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("TakeOverAsPrimaryAsync:")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        [Fact]
        public async Task TakeOverAsPrimaryAsync_TryTakeOverForPrimaryFails_LogsWarningAndReturnsFalse()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var replicationManagerMock = new Mock<IReplicationManager>();
            var clusterManagerMock = new Mock<IClusterManager>();
            var storeWrapperMock = new Mock<IStoreWrapper>();
            var clusterProviderMock = new Mock<IClusterProvider>();

            clusterProviderMock.SetupGet(c => c.replicationManager).Returns(replicationManagerMock.Object);
            clusterProviderMock.SetupGet(c => c.clusterManager).Returns(clusterManagerMock.Object);
            clusterProviderMock.SetupGet(c => c.storeWrapper).Returns(storeWrapperMock.Object);
            clusterProviderMock.Setup(c => c.BumpAndWaitForEpochTransitionAsync()).Returns(Task.CompletedTask);

            replicationManagerMock.Setup(r => r.BeginRecovery(RecoveryStatus.ClusterFailover, false)).Returns(true);
            clusterManagerMock.Setup(c => c.TryTakeOverForPrimary()).Returns(false);

            var session = new TestFailoverSession(loggerMock.Object, clusterProviderMock.Object);

            // Act
            var result = await session.TakeOverAsPrimaryAsync();

            // Assert
            Assert.False(result);
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("TakeOverAsPrimaryAsync:")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
            replicationManagerMock.Verify(r => r.EndRecovery(RecoveryStatus.NoRecovery, false), Times.Once);
        }

        [Fact]
        public async Task TakeOverAsPrimaryAsync_InitializeCheckpointStoreFails_LogsWarning()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var replicationManagerMock = new Mock<IReplicationManager>();
            var clusterManagerMock = new Mock<IClusterManager>();
            var storeWrapperMock = new Mock<IStoreWrapper>();
            var clusterProviderMock = new Mock<IClusterProvider>();

            clusterProviderMock.SetupGet(c => c.replicationManager).Returns(replicationManagerMock.Object);
            clusterProviderMock.SetupGet(c => c.clusterManager).Returns(clusterManagerMock.Object);
            clusterProviderMock.SetupGet(c => c.storeWrapper).Returns(storeWrapperMock.Object);
            clusterProviderMock.Setup(c => c.BumpAndWaitForEpochTransitionAsync()).Returns(Task.CompletedTask);

            replicationManagerMock.Setup(r => r.BeginRecovery(RecoveryStatus.ClusterFailover, false)).Returns(true);
            clusterManagerMock.Setup(c => c.TryTakeOverForPrimary()).Returns(true);
            replicationManagerMock.Setup(r => r.InitializeCheckpointStore()).Returns(false);

            var session = new TestFailoverSession(loggerMock.Object, clusterProviderMock.Object);

            // Act
            var result = await session.TakeOverAsPrimaryAsync();

            // Assert
            Assert.True(result);
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Failed acquiring latest memory checkpoint metadata")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
            replicationManagerMock.Verify(r => r.EndRecovery(RecoveryStatus.NoRecovery, false), Times.Once);
            storeWrapperMock.Verify(s => s.StartPrimaryTasks(), Times.Once);
        }
    }
}
