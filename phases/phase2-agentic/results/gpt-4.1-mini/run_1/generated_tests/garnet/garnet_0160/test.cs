using System;
using System.Net;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Garnet.cluster;
using Garnet.client;
using Garnet.common;
using Garnet.server;

namespace Garnet.Tests.cluster
{
    public class AofTaskStoreTests
    {
        // Helper to create a minimal ClusterProvider mock with required properties
        private static ClusterProvider CreateClusterProviderMock(string address = "127.0.0.1", int port = 7000)
        {
            var clusterManagerMock = new Mock<IClusterManager>();
            var currentConfigMock = new Mock<IClusterConfig>();
            currentConfigMock.Setup(c => c.LocalNodeId).Returns("localNode");
            currentConfigMock.Setup(c => c.GetWorkerAddressFromNodeId(It.IsAny<string>())).Returns((string nodeId) => (address, port));
            clusterManagerMock.Setup(c => c.CurrentConfig).Returns(currentConfigMock.Object);

            var replicationManagerMock = new Mock<IReplicationManager>();
            replicationManagerMock.Setup(r => r.GetAofSyncNetworkBufferSettings).Returns(() => null);
            replicationManagerMock.Setup(r => r.GetNetworkPool).Returns(() => null);

            var serverOptionsMock = new Mock<IServerOptions>();
            serverOptionsMock.SetupGet(s => s.TlsOptions).Returns((TlsOptions)null);

            var storeWrapperMock = new Mock<IStoreWrapper>();
            storeWrapperMock.SetupGet(s => s.appendOnlyFile).Returns((IAppendOnlyFile)null);

            var clusterProviderMock = new Mock<ClusterProvider>(
                clusterManagerMock.Object,
                replicationManagerMock.Object,
                serverOptionsMock.Object,
                storeWrapperMock.Object,
                "user",
                "pass",
                false
            );

            clusterProviderMock.SetupGet(c => c.clusterManager).Returns(clusterManagerMock.Object);
            clusterProviderMock.SetupGet(c => c.replicationManager).Returns(replicationManagerMock.Object);
            clusterProviderMock.SetupGet(c => c.serverOptions).Returns(serverOptionsMock.Object);
            clusterProviderMock.SetupGet(c => c.storeWrapper).Returns(storeWrapperMock.Object);
            clusterProviderMock.SetupGet(c => c.ClusterUsername).Returns("user");
            clusterProviderMock.SetupGet(c => c.ClusterPassword).Returns("pass");
            clusterProviderMock.SetupGet(c => c.AllowDataLoss).Returns(false);

            return clusterProviderMock.Object;
        }

        [Fact]
        public void TryAddReplicationTask_LogsError_WhenAofSyncTaskCreationThrows()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var clusterProvider = CreateClusterProviderMock();

            // We create a derived AofTaskStore that throws on AofSyncTaskInfo creation to simulate exception
            var store = new TestAofTaskStore(clusterProvider, loggerMock.Object);

            // Act
            var result = store.TryAddReplicationTask("replicaNode", 1, out var taskInfo);

            // Assert
            Assert.False(result);
            Assert.Null(taskInfo);
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("An error occurred at TryAddReplicationTask task creation")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        [Fact]
        public void TryAddReplicationTask_LogsWarning_WhenStartAddressLessThanTruncatedUntilAndNoDataLossAllowed()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var clusterProvider = CreateClusterProviderMock();

            var store = new AofTaskStore(clusterProvider, loggerMock.Object);
            // Set TruncatedUntil to 100 and startAddress less than that
            var truncatedUntilField = typeof(AofTaskStore).GetField("TruncatedUntil", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            truncatedUntilField.SetValue(store, 100L);

            // Act
            var result = store.TryAddReplicationTask("replicaNode", 50, out var taskInfo);

            // Assert
            Assert.False(result);
            Assert.Null(taskInfo);
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("AOF sync task for replicaNode")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        // Helper class to override AofSyncTaskInfo creation to throw exception
        private class TestAofTaskStore : AofTaskStore
        {
            public TestAofTaskStore(ClusterProvider clusterProvider, ILogger logger) : base(clusterProvider, 1, logger)
            {
            }

            public override bool TryAddReplicationTask(string remoteNodeId, long startAddress, out AofSyncTaskInfo aofSyncTaskInfo)
            {
                aofSyncTaskInfo = null;
                throw new Exception("Simulated exception");
            }
        }
    }
}
