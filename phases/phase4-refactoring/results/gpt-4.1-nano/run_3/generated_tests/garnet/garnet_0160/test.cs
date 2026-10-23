using System;
using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using Garnet.cluster;
using Garnet.server;
using Garnet.client;
using System.Net;

namespace Garnet.Tests
{
    public class AofTaskStoreTests
    {
        [Fact]
        public void TryAddReplicationTask_ShouldLogErrorOnException()
        {
            // Arrange
            var mockLogger = new Mock<ILogger>();
            var mockClusterProvider = new Mock<ClusterProvider>();
            var mockClusterManager = new Mock<ClusterManager>();
            var mockConfig = new Mock<ClusterConfig>();
            var mockStoreWrapper = new Mock<StoreWrapper>();
            var mockAppendOnlyFile = new Mock<AppendOnlyFile>();
            var mockServerOptions = new Mock<ServerOptions>();
            var mockTlsOptions = new Mock<TlsOptions>();
            var mockReplicationManager = new Mock<ReplicationManager>();
            var mockNetworkPool = new object();

            // Setup cluster provider
            mockClusterProvider.Setup(cp => cp.clusterManager).Returns(mockClusterManager.Object);
            mockClusterProvider.Setup(cp => cp.replicationManager).Returns(mockReplicationManager.Object);
            mockClusterProvider.Setup(cp => cp.storeWrapper).Returns(new StoreWrapper { appendOnlyFile = mockAppendOnlyFile.Object });
            mockClusterProvider.Setup(cp => cp.serverOptions).Returns(new ServerOptions { FastAofTruncate = false, TlsOptions = mockTlsOptions.Object });
            mockClusterProvider.Setup(cp => cp.ClusterUsername).Returns("user");
            mockClusterProvider.Setup(cp => cp.ClusterPassword).Returns("pass");
            mockClusterProvider.Setup(cp => cp.AllowDataLoss).Returns(false);
            mockClusterProvider.Setup(cp => cp.CurrentConfig).Returns(mockConfig.Object);

            // Setup config
            mockConfig.Setup(c => c.GetWorkerAddressFromNodeId(It.IsAny<string>())).Returns(("127.0.0.1", 7001));
            mockConfig.Setup(c => c.LocalNodeId).Returns("localNode");

            // Setup appendOnlyFile
            mockAppendOnlyFile.Setup(af => af.UnsafeGetLogPageSizeBits()).Returns(12);
            mockAppendOnlyFile.Setup(af => af.UnsafeGetReadOnlyAddressLagOffset()).Returns(4096);
            mockAppendOnlyFile.SetupSet(af => af.SafeTailShiftCallback = It.IsAny<Action<long, long>>());

            // Setup replicationManager
            mockReplicationManager.Setup(rm => rm.GetAofSyncNetworkBufferSettings).Returns(new byte[1024]);
            mockReplicationManager.Setup(rm => rm.GetNetworkPool).Returns(mockNetworkPool);

            var store = new AofTaskStore(mockClusterProvider.Object, logger: mockLogger.Object);

            // Use an invalid address to cause IPAddress.Parse to throw
            string invalidAddress = "invalid_ip";

            // Act
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
            mockLogger.Verify(
                x => x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("An error occurred at TryAddReplicationTask task creation for")),
                    It.IsAny<Exception>(),
                    (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()),
                Times.AtLeastOnce);
        }
    }
}
