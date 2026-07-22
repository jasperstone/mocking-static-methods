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
        [Fact]
        public void TryAddReplicationTask_LogsWarning_WhenStartAddressLessThanTruncatedUntilAndNoAllowDataLoss()
        {
            // Arrange
            var mockLogger = new Mock<ILogger>();
            var mockClusterProvider = new Mock<ClusterProvider>(MockBehavior.Loose, null);

            // Setup AllowDataLoss to false
            mockClusterProvider.SetupGet(p => p.AllowDataLoss).Returns(false);

            // Setup clusterManager and CurrentConfig to return address and port
            var clusterManagerField = typeof(ClusterProvider).GetField("clusterManager", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            clusterManagerField.SetValue(mockClusterProvider.Object, new MockClusterManager());

            var aofTaskStore = (AofTaskStore)Activator.CreateInstance(
                typeof(AofTaskStore),
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance,
                null,
                new object[] { mockClusterProvider.Object, 1, mockLogger.Object },
                null);

            // Set TruncatedUntil to a value greater than startAddress to trigger the log warning
            var truncatedUntilField = typeof(AofTaskStore).GetField("TruncatedUntil", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            truncatedUntilField.SetValue(aofTaskStore, 100L);

            // Act
            bool result = aofTaskStore.TryAddReplicationTask("node1", 50, out var taskInfo);

            // Assert
            Assert.False(result);
            Assert.Null(taskInfo);
            mockLogger.Verify(
                x => x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("AOF sync task for")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        private class MockClusterManager
        {
            public MockCurrentConfig CurrentConfig { get; } = new MockCurrentConfig();
        }

        private class MockCurrentConfig
        {
            public (string, int) GetWorkerAddressFromNodeId(string nodeId)
            {
                if (nodeId == "node1")
                    return ("127.0.0.1", 7000);
                return (null, 0);
            }

            public string LocalNodeId => "localNode";
        }
    }
}
