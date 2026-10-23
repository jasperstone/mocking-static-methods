using System;
using System.Text;
using System.Threading.Tasks;
using Garnet.cluster;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Garnet.cluster
{
    internal class FailoverSessionTests
    {
        private FailoverSession CreateFailoverSessionWithMocks(
            Mock<IClusterProvider> mockClusterProvider,
            Mock<ILogger> mockLogger)
        {
            var failoverTimeout = TimeSpan.FromSeconds(10);
            var clusterTimeout = TimeSpan.FromSeconds(10);
            var epoch = new client.LightEpoch();
            var option = FailoverOption.None;

            return new FailoverSession(
                mockClusterProvider.Object,
                option,
                clusterTimeout,
                failoverTimeout,
                epoch,
                isReplicaSession: true,
                logger: mockLogger.Object);
        }

        private Task<bool> InvokeTakeOverAsPrimaryAsync(FailoverSession session)
        {
            var method = typeof(FailoverSession).GetMethod("TakeOverAsPrimaryAsync", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            return (Task<bool>)method.Invoke(session, null);
        }

        [Fact]
        public async Task TakeOverAsPrimaryAsync_LogsWarning_WhenBeginRecoveryFails()
        {
            // Arrange
            var mockLogger = new Mock<ILogger>();
            var mockReplicationManager = new Mock<IReplicationManager>();
            var mockClusterManager = new Mock<IClusterManager>();
            var mockStoreWrapper = new Mock<IStoreWrapper>();
            var mockClusterProvider = new Mock<IClusterProvider>();

            // Setup BeginRecovery to return false to simulate failure to acquire recovery lock
            mockReplicationManager.Setup(r => r.BeginRecovery(It.IsAny<RecoveryStatus>(), false)).Returns(false);

            mockClusterProvider.SetupGet(c => c.replicationManager).Returns(mockReplicationManager.Object);
            mockClusterProvider.SetupGet(c => c.clusterManager).Returns(mockClusterManager.Object);
            mockClusterProvider.SetupGet(c => c.storeWrapper).Returns(mockStoreWrapper.Object);
            mockClusterProvider.Setup(c => c.BumpAndWaitForEpochTransitionAsync()).Returns(Task.CompletedTask);
            mockClusterProvider.SetupGet(c => c.serverOptions).Returns(new ServerOptions());

            var session = CreateFailoverSessionWithMocks(mockClusterProvider, mockLogger);

            // Act
            var result = await InvokeTakeOverAsPrimaryAsync(session);

            // Assert
            Assert.False(result);
            mockLogger.Verify(
                x => x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("TakeOverAsPrimaryAsync:")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        [Fact]
        public async Task TakeOverAsPrimaryAsync_LogsWarning_WhenTryTakeOverForPrimaryFails()
        {
            // Arrange
            var mockLogger = new Mock<ILogger>();
            var mockReplicationManager = new Mock<IReplicationManager>();
            var mockClusterManager = new Mock<IClusterManager>();
            var mockStoreWrapper = new Mock<IStoreWrapper>();
            var mockClusterProvider = new Mock<IClusterProvider>();

            // Setup BeginRecovery to return true to proceed
            mockReplicationManager.Setup(r => r.BeginRecovery(It.IsAny<RecoveryStatus>(), false)).Returns(true);
            // Setup TryTakeOverForPrimary to return false to simulate failure
            mockClusterManager.Setup(c => c.TryTakeOverForPrimary()).Returns(false);

            mockClusterProvider.SetupGet(c => c.replicationManager).Returns(mockReplicationManager.Object);
            mockClusterProvider.SetupGet(c => c.clusterManager).Returns(mockClusterManager.Object);
            mockClusterProvider.SetupGet(c => c.storeWrapper).Returns(mockStoreWrapper.Object);
            mockClusterProvider.Setup(c => c.BumpAndWaitForEpochTransitionAsync()).Returns(Task.CompletedTask);
            mockClusterProvider.SetupGet(c => c.serverOptions).Returns(new ServerOptions());

            var session = CreateFailoverSessionWithMocks(mockClusterProvider, mockLogger);

            // Act
            var result = await InvokeTakeOverAsPrimaryAsync(session);

            // Assert
            Assert.False(result);
            mockLogger.Verify(
                x => x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("TakeOverAsPrimaryAsync:")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
            // Verify EndRecovery is called in finally
            mockReplicationManager.Verify(r => r.EndRecovery(RecoveryStatus.NoRecovery, false), Times.Once);
        }
    }

    // Interfaces to mock nested dependencies used in FailoverSession
    internal interface IReplicationManager
    {
        bool BeginRecovery(RecoveryStatus status, bool upgradeLock);
        void EndRecovery(RecoveryStatus status, bool downgradeLock);
        void TryUpdateForFailover();
        void ResetReplayIterator();
        bool InitializeCheckpointStore();
    }

    internal interface IClusterManager
    {
        bool TryTakeOverForPrimary();
        object CurrentConfig { get; }
    }

    internal interface IStoreWrapper
    {
        void StartPrimaryTasks();
    }

    internal interface IClusterProvider
    {
        IReplicationManager replicationManager { get; }
        IClusterManager clusterManager { get; }
        IStoreWrapper storeWrapper { get; }
        Task BumpAndWaitForEpochTransitionAsync();
        ServerOptions serverOptions { get; }
        string ClusterUsername { get; }
        string ClusterPassword { get; }
    }

    internal enum RecoveryStatus
    {
        ClusterFailover,
        NoRecovery
    }

    internal class ServerOptions
    {
        public TlsOptions TlsOptions { get; set; }
        public int ClusterTimeout { get; set; }
    }

    internal class TlsOptions
    {
        public object TlsClientOptions { get; set; }
    }

    internal enum FailoverOption
    {
        None
    }
}
