using System;
using System.Net;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Garnet.cluster;
using Garnet.client;

namespace Garnet.Tests.cluster
{
    public class AofTaskStoreTests
    {
        private class DummyClusterConfig
        {
            public string LocalNodeId { get; set; } = "localNode";
            public (string, int) GetWorkerAddressFromNodeId(string nodeId) => ("127.0.0.1", 7000);
        }

        private class DummyClusterManager
        {
            public DummyClusterConfig CurrentConfig { get; set; } = new DummyClusterConfig();
        }

        private class DummyReplicationManager
        {
            public Func<object> GetAofSyncNetworkBufferSettings { get; set; } = () => null;
            public Func<object> GetNetworkPool { get; set; } = () => null;
        }

        private class DummyServerOptions
        {
            public bool FastAofTruncate { get; set; } = false;
            public object TlsOptions { get; set; } = null;
            public string ClusterUsername { get; set; } = "user";
            public string ClusterPassword { get; set; } = "pass";
        }

        private class DummyStoreWrapper
        {
            public object appendOnlyFile => null;
            public DummyServerOptions serverOptions { get; } = new DummyServerOptions();
            public ILoggerFactory loggerFactory { get; } = null;
        }

        private class DummyClusterProvider : ClusterProvider
        {
            public DummyClusterManager clusterManagerMock = new DummyClusterManager();
            public DummyReplicationManager replicationManagerMock = new DummyReplicationManager();
            public DummyStoreWrapper storeWrapperMock = new DummyStoreWrapper();
            public DummyServerOptions serverOptionsMock = new DummyServerOptions();

            public DummyClusterProvider() : base(null)
            {
                var clusterManagerField = typeof(ClusterProvider).GetField("clusterManager", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                var replicationManagerField = typeof(ClusterProvider).GetField("replicationManager", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                var storeWrapperField = typeof(ClusterProvider).GetField("storeWrapper", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                var serverOptionsField = typeof(ClusterProvider).GetField("serverOptions", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

                clusterManagerField.SetValue(this, clusterManagerMock);
                replicationManagerField.SetValue(this, replicationManagerMock);
                storeWrapperField.SetValue(this, storeWrapperMock);
                serverOptionsField.SetValue(this, serverOptionsMock);
            }

            public override string ClusterUsername => serverOptionsMock.ClusterUsername;
            public override string ClusterPassword => serverOptionsMock.ClusterPassword;
            public override bool AllowDataLoss => false;
        }

        [Fact]
        public void TryAddReplicationTask_LogsWarning_WhenStartAddressLessThanTruncatedUntilAndNoAllowDataLoss()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var clusterProvider = new DummyClusterProvider();
            var aofTaskStore = new AofTaskStore(clusterProvider, logger: loggerMock.Object);

            // Set TruncatedUntil to a value greater than startAddress
            var truncatedUntilField = typeof(AofTaskStore).GetField("TruncatedUntil", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            truncatedUntilField.SetValue(aofTaskStore, 20L);

            // Add a dummy task to avoid _disposed early return
            var dummyTaskInfo = new AofSyncTaskInfo(
                clusterProvider,
                aofTaskStore,
                "localNode",
                "replicaNode",
                new GarnetClientSession(
                    new IPEndPoint(IPAddress.Parse("127.0.0.1"), 7000),
                    clusterProvider.replicationManagerMock.GetAofSyncNetworkBufferSettings,
                    clusterProvider.replicationManagerMock.GetNetworkPool,
                    tlsOptions: null,
                    authUsername: "user",
                    authPassword: "pass",
                    logger: loggerMock.Object),
                30,
                loggerMock.Object);

            var tasksField = typeof(AofTaskStore).GetField("tasks", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var numTasksField = typeof(AofTaskStore).GetField("numTasks", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            tasksField.SetValue(aofTaskStore, new[] { dummyTaskInfo });
            numTasksField.SetValue(aofTaskStore, 1);

            // Act
            var result = aofTaskStore.TryAddReplicationTask("replicaNode", 10L, out var aofSyncTaskInfo);

            // Assert
            Assert.False(result);
            Assert.Null(aofSyncTaskInfo);
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("could not be added")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }
    }
}
