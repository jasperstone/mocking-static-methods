using System;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Garnet.cluster
{
    public class FailoverSessionTests
    {
        // Mock interfaces based on usage in the method
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
        }

        public interface IStoreWrapper
        {
            void StartPrimaryTasks();
        }

        public interface IClusterProvider
        {
            IReplicationManager replicationManager { get; }
            IClusterManager clusterManager { get; }
            Task BumpAndWaitForEpochTransitionAsync();
            IStoreWrapper storeWrapper { get; }
        }

        public enum RecoveryStatus
        {
            ClusterFailover,
            NoRecovery
        }

        private static async Task<bool> InvokeTakeOverAsPrimaryAsync(object instance)
        {
            var method = instance.GetType().GetMethod("TakeOverAsPrimaryAsync", BindingFlags.NonPublic | BindingFlags.Instance);
            var task = (Task<bool>)method.Invoke(instance, null);
            return await task.ConfigureAwait(false);
        }

        private class FailoverSessionAccessor
        {
            public object Instance { get; }
            public Mock<IClusterProvider> ClusterProviderMock { get; }
            public Mock<ILogger> LoggerMock { get; }

            public FailoverSessionAccessor()
            {
                // Create mocks
                ClusterProviderMock = new Mock<IClusterProvider>();
                LoggerMock = new Mock<ILogger>();

                // Create instance of FailoverSession via reflection
                var failoverSessionType = typeof(FailoverSession);
                Instance = System.Runtime.Serialization.FormatterServices.GetUninitializedObject(failoverSessionType);

                // Set the private fields clusterProvider and logger via reflection
                var clusterProviderField = failoverSessionType.GetField("clusterProvider", BindingFlags.NonPublic | BindingFlags.Instance);
                clusterProviderField.SetValue(Instance, ClusterProviderMock.Object);

                var loggerField = failoverSessionType.GetField("logger", BindingFlags.NonPublic | BindingFlags.Instance);
                loggerField.SetValue(Instance, LoggerMock.Object);
            }
        }

        [Fact]
        public async Task TakeOverAsPrimaryAsync_LogsWarning_WhenCannotAcquireRecoveryLock()
        {
            var accessor = new FailoverSessionAccessor();

            var replicationManagerMock = new Mock<IReplicationManager>();
            replicationManagerMock.Setup(r => r.BeginRecovery(RecoveryStatus.ClusterFailover, false)).Returns(false);

            accessor.ClusterProviderMock.SetupGet(c => c.replicationManager).Returns(replicationManagerMock.Object);
            accessor.ClusterProviderMock.SetupGet(c => c.clusterManager).Returns(Mock.Of<IClusterManager>());
            accessor.ClusterProviderMock.Setup(c => c.BumpAndWaitForEpochTransitionAsync()).Returns(Task.CompletedTask);
            accessor.ClusterProviderMock.SetupGet(c => c.storeWrapper).Returns(Mock.Of<IStoreWrapper>());

            var result = await InvokeTakeOverAsPrimaryAsync(accessor.Instance);

            Assert.False(result);

            accessor.LoggerMock.Verify(
                x => x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("TakeOverAsPrimaryAsync:")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        [Fact]
        public async Task TakeOverAsPrimaryAsync_LogsWarning_WhenCannotTakeOverFromPrimary()
        {
            var accessor = new FailoverSessionAccessor();

            var replicationManagerMock = new Mock<IReplicationManager>();
            replicationManagerMock.Setup(r => r.BeginRecovery(RecoveryStatus.ClusterFailover, false)).Returns(true);
            replicationManagerMock.Setup(r => r.EndRecovery(RecoveryStatus.NoRecovery, false));

            var clusterManagerMock = new Mock<IClusterManager>();
            clusterManagerMock.Setup(c => c.TryTakeOverForPrimary()).Returns(false);

            accessor.ClusterProviderMock.SetupGet(c => c.replicationManager).Returns(replicationManagerMock.Object);
            accessor.ClusterProviderMock.SetupGet(c => c.clusterManager).Returns(clusterManagerMock.Object);
            accessor.ClusterProviderMock.Setup(c => c.BumpAndWaitForEpochTransitionAsync()).Returns(Task.CompletedTask);
            accessor.ClusterProviderMock.SetupGet(c => c.storeWrapper).Returns(Mock.Of<IStoreWrapper>());

            var result = await InvokeTakeOverAsPrimaryAsync(accessor.Instance);

            Assert.False(result);

            accessor.LoggerMock.Verify(
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
        public async Task TakeOverAsPrimaryAsync_LogsWarning_WhenInitializeCheckpointStoreFails()
        {
            var accessor = new FailoverSessionAccessor();

            var replicationManagerMock = new Mock<IReplicationManager>();
            replicationManagerMock.Setup(r => r.BeginRecovery(RecoveryStatus.ClusterFailover, false)).Returns(true);
            replicationManagerMock.Setup(r => r.EndRecovery(RecoveryStatus.NoRecovery, false));
            replicationManagerMock.Setup(r => r.InitializeCheckpointStore()).Returns(false);
            replicationManagerMock.Setup(r => r.TryUpdateForFailover());
            replicationManagerMock.Setup(r => r.ResetReplayIterator());

            var clusterManagerMock = new Mock<IClusterManager>();
            clusterManagerMock.Setup(c => c.TryTakeOverForPrimary()).Returns(true);

            var storeWrapperMock = new Mock<IStoreWrapper>();
            storeWrapperMock.Setup(s => s.StartPrimaryTasks());

            accessor.ClusterProviderMock.SetupGet(c => c.replicationManager).Returns(replicationManagerMock.Object);
            accessor.ClusterProviderMock.SetupGet(c => c.clusterManager).Returns(clusterManagerMock.Object);
            accessor.ClusterProviderMock.Setup(c => c.BumpAndWaitForEpochTransitionAsync()).Returns(Task.CompletedTask);
            accessor.ClusterProviderMock.SetupGet(c => c.storeWrapper).Returns(storeWrapperMock.Object);

            var result = await InvokeTakeOverAsPrimaryAsync(accessor.Instance);

            Assert.True(result);

            accessor.LoggerMock.Verify(
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
