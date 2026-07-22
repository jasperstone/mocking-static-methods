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
        private class StubCurrentConfig
        {
            public string LocalNodeId => "localNode";

            public (string, int) GetWorkerAddressFromNodeId(string nodeId)
            {
                if (nodeId == "validNode")
                    return ("127.0.0.1", 7000);
                if (nodeId == "nullAddress")
                    return (null, 7000);
                if (nodeId == "invalidPort")
                    return ("127.0.0.1", 0);
                return ("127.0.0.1", 7000);
            }
        }

        private class StubClusterManager
        {
            public StubCurrentConfig CurrentConfig { get; } = new StubCurrentConfig();
        }

        private class StubReplicationManager
        {
            public Func<int> GetAofSyncNetworkBufferSettings => () => 0;
            public Func<int> GetNetworkPool => () => 0;
        }

        private class StubStoreWrapper
        {
            public StubAppendOnlyFile appendOnlyFile { get; } = new StubAppendOnlyFile();
            public object serverOptions { get; } = new { FastAofTruncate = false, TlsOptions = (object)null };
        }

        private class StubAppendOnlyFile
        {
            public int UnsafeGetLogPageSizeBits() => 12;
            public long UnsafeGetReadOnlyAddressLagOffset() => 4096;
            public Action<long, long> SafeTailShiftCallback { get; set; }
        }

        private class StubServerOptions
        {
            public object TlsOptions => null;
            public bool FastAofTruncate => false;
            public string ClusterUsername => "user";
            public string ClusterPassword => "pass";
        }

        private class FakeClusterProvider : ClusterProvider
        {
            public StubClusterManager clusterManagerField;
            public StubReplicationManager replicationManagerField;
            public StubStoreWrapper storeWrapperField;
            public StubServerOptions serverOptionsField;
            public bool allowDataLossField;

            public FakeClusterProvider() : base(null)
            {
                clusterManagerField = new StubClusterManager();
                replicationManagerField = new StubReplicationManager();
                storeWrapperField = new StubStoreWrapper();
                serverOptionsField = new StubServerOptions();
                allowDataLossField = false;

                var clusterManagerFieldInfo = typeof(ClusterProvider).GetField("clusterManager", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                clusterManagerFieldInfo.SetValue(this, clusterManagerField);
                var replicationManagerFieldInfo = typeof(ClusterProvider).GetField("replicationManager", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                replicationManagerFieldInfo.SetValue(this, replicationManagerField);
                var storeWrapperFieldInfo = typeof(ClusterProvider).GetField("storeWrapper", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                storeWrapperFieldInfo.SetValue(this, storeWrapperField);
                var serverOptionsFieldInfo = typeof(ClusterProvider).GetField("serverOptions", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                serverOptionsFieldInfo.SetValue(this, serverOptionsField);
            }

            public bool AllowDataLossOverride
            {
                get => allowDataLossField;
                set => allowDataLossField = value;
            }

            public bool GetAllowDataLoss()
            {
                return allowDataLossField;
            }
        }

        [Fact]
        public void TryAddReplicationTask_LogsWarning_WhenStartAddressLessThanTruncatedUntilAndNoAllowDataLoss()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var clusterProvider = new FakeClusterProvider();

            var store = new AofTaskStore(clusterProvider, logger: loggerMock.Object);

            // Set TruncatedUntil to a value greater than startAddress to trigger the log warning
            var truncatedUntilField = typeof(AofTaskStore).GetField("TruncatedUntil", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            truncatedUntilField.SetValue(store, 100L);

            // Act
            bool result = store.TryAddReplicationTask("validNode", 50, out var taskInfo);

            // Assert
            Assert.False(result);
            Assert.Null(taskInfo);
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
