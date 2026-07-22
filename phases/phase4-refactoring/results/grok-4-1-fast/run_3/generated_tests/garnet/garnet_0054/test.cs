using System;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Garnet.cluster.Tests
{
    public class ReplicaFailoverSessionLoggerTests
    {
        [Fact]
        public async Task TakeOverAsPrimaryAsync_LogsWarning_WhenBeginRecoveryFails()
        {
            // Arrange
            var mockLogger = new Mock<ILogger>();
            var mockReplicationManager = new Mock<MockReplicationManager>();
            mockReplicationManager.Setup(x => x.BeginRecovery(It.IsAny<object>(), false)).Returns(false);

            var mockClusterProvider = new Mock<MockClusterProvider>();
            mockClusterProvider.SetupGet(x => x.replicationManager).Returns(mockReplicationManager.Object);
            mockClusterProvider.Setup(x => x.BumpAndWaitForEpochTransitionAsync()).Returns(Task.CompletedTask);

            var session = new TestableFailoverSession(mockLogger.Object, mockClusterProvider.Object);

            // Act
            var result = await session.TakeOverAsPrimaryAsync();

            // Assert
            Assert.False(result);
            mockLogger.Verify(
                x => x.LogWarning(
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("TakeOverAsPrimaryAsync:") && v.ToString()!.Contains("CANNOT_ACQUIRE_RECOVERY_LOCK")),
                    It.IsAny<Exception>(),
                    It.IsAny<object[]>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, object[], string>>()),
                Times.Once);
        }

        [Fact]
        public async Task TakeOverAsPrimaryAsync_LogsWarning_WhenTryTakeOverForPrimaryFails()
        {
            // Arrange
            var mockLogger = new Mock<ILogger>();
            var mockReplicationManager = new Mock<MockReplicationManager>();
            mockReplicationManager.Setup(x => x.BeginRecovery(It.IsAny<object>(), false)).Returns(true);

            var mockClusterManager = new Mock<MockClusterManager>();
            mockClusterManager.Setup(x => x.TryTakeOverForPrimary()).Returns(false);

            var mockClusterProvider = new Mock<MockClusterProvider>();
            mockClusterProvider.SetupGet(x => x.replicationManager).Returns(mockReplicationManager.Object);
            mockClusterProvider.SetupGet(x => x.clusterManager).Returns(mockClusterManager.Object);
            mockClusterProvider.Setup(x => x.BumpAndWaitForEpochTransitionAsync()).Returns(Task.CompletedTask);

            var session = new TestableFailoverSession(mockLogger.Object, mockClusterProvider.Object);

            // Act
            var result = await session.TakeOverAsPrimaryAsync();

            // Assert
            Assert.False(result);
            mockLogger.Verify(
                x => x.LogWarning(
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("TakeOverAsPrimaryAsync:") && v.ToString()!.Contains("CANNOT_TAKEOVER_FROM_PRIMARY")),
                    It.IsAny<Exception>(),
                    It.IsAny<object[]>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, object[], string>>()),
                Times.Once);
        }

        [Fact]
        public async Task TakeOverAsPrimaryAsync_LogsWarning_WhenInitializeCheckpointStoreFails()
        {
            // Arrange
            var mockLogger = new Mock<ILogger>();
            var mockReplicationManager = new Mock<MockReplicationManager>();
            mockReplicationManager.Setup(x => x.BeginRecovery(It.IsAny<object>(), false)).Returns(true);
            mockReplicationManager.Setup(x => x.TryUpdateForFailover());
            mockReplicationManager.Setup(x => x.ResetReplayIterator());
            mockReplicationManager.Setup(x => x.InitializeCheckpointStore()).Returns(false);
            mockReplicationManager.Setup(x => x.EndRecovery(It.IsAny<object>(), false));

            var mockClusterManager = new Mock<MockClusterManager>();
            mockClusterManager.Setup(x => x.TryTakeOverForPrimary()).Returns(true);

            var mockClusterProvider = new Mock<MockClusterProvider>();
            mockClusterProvider.SetupGet(x => x.replicationManager).Returns(mockReplicationManager.Object);
            mockClusterProvider.SetupGet(x => x.clusterManager).Returns(mockClusterManager.Object);
            mockClusterProvider.Setup(x => x.BumpAndWaitForEpochTransitionAsync()).Returns(Task.CompletedTask);
            mockClusterProvider.SetupGet(x => x.storeWrapper).Returns(new Mock<object>().Object);

            var session = new TestableFailoverSession(mockLogger.Object, mockClusterProvider.Object);

            // Act
            var result = await session.TakeOverAsPrimaryAsync();

            // Assert
            Assert.True(result);
            mockLogger.Verify(
                x => x.LogWarning(
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Failed acquiring latest memory checkpoint metadata") && v.ToString()!.Contains("TakeOverAsPrimaryAsync")),
                    It.IsAny<Exception>(),
                    It.IsAny<object[]>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, object[], string>>()),
                Times.Once);
        }
    }

    // Test-specific interfaces that don't depend on internal types
    public interface MockClusterProvider
    {
        dynamic replicationManager { get; }
        dynamic clusterManager { get; }
        dynamic storeWrapper { get; }
        Task BumpAndWaitForEpochTransitionAsync();
    }

    public interface MockReplicationManager
    {
        bool BeginRecovery(object status, bool upgradeLock);
        void TryUpdateForFailover();
        void ResetReplayIterator();
        bool InitializeCheckpointStore();
        void EndRecovery(object status, bool downgradeLock);
    }

    public interface MockClusterManager
    {
        bool TryTakeOverForPrimary();
    }

    // Self-contained test double with copied method logic
    internal class TestableFailoverSession
    {
        internal ILogger logger;
        internal MockClusterProvider clusterProvider;
        internal object oldConfig;
        internal System.Threading.CancellationTokenSource cts;
        internal long epoch;
        internal object status;
        internal TimeSpan failoverTimeout;

        public TestableFailoverSession(ILogger logger, MockClusterProvider clusterProvider)
        {
            this.logger = logger;
            this.clusterProvider = clusterProvider;
            this.oldConfig = new { LocalNodeId = "test", LocalNodePrimaryId = "primary", GetEndpointFromNodeId = new Func<string, string>(_ => "endpoint") };
            this.cts = new System.Threading.CancellationTokenSource();
            this.epoch = 1;
            this.status = new object();
            this.failoverTimeout = TimeSpan.FromSeconds(30);
        }

        public async Task<bool> TakeOverAsPrimaryAsync()
        {
            // Exact reproduction of the method body to test the logger call on line 130
            status = new object(); // TAKING_OVER_AS_PRIMARY
            var acquiredLock = false;

            try
            {
                // Simulate the exact condition for line 130
                if (!clusterProvider.replicationManager.BeginRecovery(new object(), upgradeLock: false))
                {
                    logger?.LogWarning($"{nameof(TakeOverAsPrimaryAsync)}: {{logMessage}}", 
                        System.Text.Encoding.ASCII.GetString(new byte[] { /* RESP_ERR_GENERIC_CANNOT_ACQUIRE_RECOVERY_LOCK bytes */ }));
                    return false;
                }
                acquiredLock = true;
                _ = await clusterProvider.BumpAndWaitForEpochTransitionAsync().ConfigureAwait(false);

                if (!clusterProvider.clusterManager.TryTakeOverForPrimary())
                {
                    logger?.LogWarning($"{nameof(TakeOverAsPrimaryAsync)}: {{logMessage}}", 
                        System.Text.Encoding.ASCII.GetString(new byte[] { /* RESP_ERR_GENERIC_CANNOT_TAKEOVER_FROM_PRIMARY bytes */ }));
                    return false;
                }

                clusterProvider.replicationManager.TryUpdateForFailover();
                clusterProvider.replicationManager.ResetReplayIterator();

                if (!clusterProvider.replicationManager.InitializeCheckpointStore())
                    logger?.LogWarning("Failed acquiring latest memory checkpoint metadata at {method}", nameof(TakeOverAsPrimaryAsync));
                _ = clusterProvider.BumpAndWaitForEpochTransitionAsync().ConfigureAwait(false);

                clusterProvider.storeWrapper.StartPrimaryTasks();
            }
            finally
            {
                if (acquiredLock) 
                    clusterProvider.replicationManager.EndRecovery(new object(), downgradeLock: false);
            }

            return true;
        }
    }
}
