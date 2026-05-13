using System;
using System.Net;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Garnet.cluster;
using Garnet.client;
using Garnet.server;

namespace Garnet.Tests.cluster
{
    public class AofTaskStoreTests
    {
        // Helper to create a minimal ClusterProvider mock with required properties
        private static ClusterProvider CreateClusterProviderMock(string address = "127.0.0.1", int port = 7000, bool allowDataLoss = false)
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
            serverOptionsMock.SetupGet(s => s.TlsOptions).Returns((ITlsOptions)null);

            var storeWrapperMock = new Mock<IStoreWrapper>();
            storeWrapperMock.SetupGet(s => s.appendOnlyFile).Returns((IAppendOnlyFile)null);

            var clusterProviderMock = new Mock<ClusterProvider>();
            clusterProviderMock.SetupGet(c => c.clusterManager).Returns(clusterManagerMock.Object);
            clusterProviderMock.SetupGet(c => c.replicationManager).Returns(replicationManagerMock.Object);
            clusterProviderMock.SetupGet(c => c.serverOptions).Returns(serverOptionsMock.Object);
            clusterProviderMock.SetupGet(c => c.storeWrapper).Returns(storeWrapperMock.Object);
            clusterProviderMock.SetupGet(c => c.ClusterUsername).Returns("user");
            clusterProviderMock.SetupGet(c => c.ClusterPassword).Returns("pass");
            clusterProviderMock.SetupGet(c => c.AllowDataLoss).Returns(allowDataLoss);

            return clusterProviderMock.Object;
        }

        [Fact]
        public void TryAddReplicationTask_LogsError_WhenTaskCreationThrows()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var clusterProvider = CreateClusterProviderMock();

            var aofTaskStore = new AofTaskStore(clusterProvider, logger: loggerMock.Object);

            // We simulate invalid IP address to cause IPAddress.Parse to throw inside TryAddReplicationTask
            var invalidAddress = "invalid_ip";
            var clusterManagerMock = new Mock<IClusterManager>();
            var currentConfigMock = new Mock<IClusterConfig>();
            currentConfigMock.Setup(c => c.LocalNodeId).Returns("localNode");
            currentConfigMock.Setup(c => c.GetWorkerAddressFromNodeId(It.IsAny<string>())).Returns((string nodeId) => (invalidAddress, 7000));
            clusterManagerMock.Setup(c => c.CurrentConfig).Returns(currentConfigMock.Object);

            var clusterProviderMock = new Mock<ClusterProvider>();
            clusterProviderMock.SetupGet(c => c.clusterManager).Returns(clusterManagerMock.Object);
            clusterProviderMock.SetupGet(c => c.replicationManager).Returns(Mock.Of<IReplicationManager>());
            clusterProviderMock.SetupGet(c => c.serverOptions).Returns(Mock.Of<IServerOptions>());
            clusterProviderMock.SetupGet(c => c.storeWrapper).Returns(Mock.Of<IStoreWrapper>());
            clusterProviderMock.SetupGet(c => c.ClusterUsername).Returns("user");
            clusterProviderMock.SetupGet(c => c.ClusterPassword).Returns("pass");
            clusterProviderMock.SetupGet(c => c.AllowDataLoss).Returns(false);

            var aofTaskStoreWithInvalidAddress = new AofTaskStore(clusterProviderMock.Object, logger: loggerMock.Object);

            // Act
            var result = aofTaskStoreWithInvalidAddress.TryAddReplicationTask("replicaNode", 1, out var taskInfo);

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
        public void TryAddReplicationTask_LogsWarning_WhenStartAddressLessThanTruncatedUntilAndNoAllowDataLoss()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var clusterProvider = CreateClusterProviderMock(allowDataLoss: false);

            var aofTaskStore = new AofTaskStore(clusterProvider, logger: loggerMock.Object);

            // Set TruncatedUntil to a value greater than startAddress to trigger the warning
            var truncatedUntilField = typeof(AofTaskStore).GetField("TruncatedUntil", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            truncatedUntilField.SetValue(aofTaskStore, 100L);

            // Act
            var result = aofTaskStore.TryAddReplicationTask("replicaNode", 50, out var taskInfo);

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
    }
}
