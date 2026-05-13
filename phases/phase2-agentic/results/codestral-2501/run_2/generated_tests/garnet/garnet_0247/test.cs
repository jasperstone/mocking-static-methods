using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using Garnet.cluster;
using System;

namespace Garnet.cluster.Tests
{
    public class ReplicationReplicaAofSyncTests
    {
        [Fact]
        public void ProcessPrimaryStream_ExceptionThrown_LogsWarning()
        {
            // Arrange
            var mockLogger = new Mock<ILogger<ReplicationManager>>();
            var mockClusterProvider = new Mock<ClusterProvider>();
            var mockStoreWrapper = new Mock<StoreWrapper>();
            var mockAppendOnlyFile = new Mock<AppendOnlyFile>();
            var mockServerOptions = new Mock<ServerOptions>();
            var mockClusterManager = new Mock<ClusterManager>();
            var mockCurrentConfig = new Mock<ClusterConfig>();

            mockClusterProvider.Setup(cp => cp.storeWrapper).Returns(mockStoreWrapper.Object);
            mockClusterProvider.Setup(cp => cp.serverOptions).Returns(mockServerOptions.Object);
            mockClusterProvider.Setup(cp => cp.clusterManager).Returns(mockClusterManager.Object);
            mockStoreWrapper.Setup(sw => sw.appendOnlyFile).Returns(mockAppendOnlyFile.Object);
            mockClusterManager.Setup(cm => cm.CurrentConfig).Returns(mockCurrentConfig.Object);

            var replicationManager = new ReplicationManager(mockClusterProvider.Object, mockLogger.Object);

            // Act
            try
            {
                replicationManager.ProcessPrimaryStream(IntPtr.Zero, 0, 0, 0, 0);
            }
            catch (GarnetException)
            {
                // Expected exception
            }

            // Assert
            mockLogger.Verify(
                x => x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("An exception occurred at ReplicationManager.ProcessPrimaryStream")),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
                Times.Once);
        }
    }
}
