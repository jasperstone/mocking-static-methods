using System;
using System.Net;
using Xunit;
using Moq;
using Garnet.cluster;
using Garnet.server;
using Garnet.common;
using Microsoft.Extensions.Logging;

namespace Garnet.Tests
{
    public class AofTaskStoreTests
    {
        [Fact]
        public void TryAddReplicationTask_Should_LogError_When_ExceptionOccurs()
        {
            // Arrange
            var mockLogger = new Mock<ILogger>();
            var mockClusterProvider = new Mock<ClusterProvider>();
            var mockClusterManager = new Mock<ClusterManager>();
            var mockReplicationManager = new Mock<ReplicationManager>();
            var mockStoreWrapper = new Mock<StoreWrapper>();
            var mockAppendOnlyFile = new Mock<AppendOnlyFile>();
            var mockServerOptions = new Mock<ServerOptions>();
            var mockTlsOptions = new TlsOptions();

            // Setup cluster provider
            mockClusterProvider.Setup(cp => cp.clusterManager).Returns(mockClusterManager.Object);
            mockClusterProvider.Setup(cp => cp.replicationManager).Returns(mockReplicationManager.Object);
            mockClusterProvider.Setup(cp => cp.storeWrapper).Returns(mockStoreWrapper.Object);
            mockClusterProvider.Setup(cp => cp.serverOptions).Returns(mockServerOptions.Object);
            mockClusterProvider.Setup(cp => cp.ClusterUsername).Returns("user");
            mockClusterProvider.Setup(cp => cp.ClusterPassword).Returns("pass");
            mockClusterProvider.Setup(cp => cp.AllowDataLoss).Returns(false);
            mockClusterProvider.Setup(cp => cp.CurrentConfig).Returns(new ClusterConfig());

            // Setup store wrapper and append only file
            mockStoreWrapper.Setup(sw => sw.appendOnlyFile).Returns(mockAppendOnlyFile.Object);
            mockAppendOnlyFile.Setup(af => af.UnsafeGetLogPageSizeBits()).Returns(12);
            mockAppendOnlyFile.Setup(af => af.UnsafeGetReadOnlyAddressLagOffset()).Returns(8192);
            mockAppendOnlyFile.SetupSet(af => af.SafeTailShiftCallback = It.IsAny<Action<long, long>>());

            // Setup cluster manager config
            var currentConfig = new ClusterConfig();
            mockClusterManager.Setup(cm => cm.CurrentConfig).Returns(currentConfig);
            currentConfig.SetWorkerAddress("node1", "127.0.0.1", 7001);
            currentConfig.SetWorkerAddress("node2", "127.0.0.1", 7002);

            // Create AofTaskStore
            var store = new AofTaskStore(mockClusterProvider.Object, logger: mockLogger.Object);

            // Setup the cluster config to return address and port
            mockClusterProvider.Setup(cp => cp.CurrentConfig.GetWorkerAddressFromNodeId(It.IsAny<string>()))
                .Returns((string nodeId) => ("127.0.0.1", 7001));

            // Act
            // We will cause an exception during AofSyncTaskInfo creation by passing an invalid address
            var result = false;
            try
            {
                result = store.TryAddReplicationTask("node1", 0, out var task);
            }
            catch (Exception)
            {
                // ignore
            }

            // Assert
            // Verify that LogError was called with the expected message
            mockLogger.Verify(
                logger => logger.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("creating AOF sync task for")),
                    It.IsAny<Exception>(),
                    (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()),
                Times.AtLeastOnce);
        }
    }
}
