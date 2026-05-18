using Xunit;
using Moq;
using System;
using System.Threading;
using System.Threading.Tasks;
using Garnet.cluster;
using Microsoft.Extensions.Logging;

namespace Garnet.Tests
{
    public class ReplicationManagerTests
    {
        [Fact]
        public void ProcessPrimaryStream_Should_LogWarning_When_DivergentAOFStreamExceptionOccurs()
        {
            // Arrange
            var mockLogger = new Mock<ILogger<ReplicationManager>>();
            var mockStoreWrapper = new Mock<IStoreWrapper>();
            var mockAppendOnlyFile = new Mock<IAppendOnlyFile>();
            var mockClusterProvider = new Mock<IClusterProvider>();
            var mockClusterManager = new Mock<IClusterManager>();
            var mockActiveReplay = new Mock<IActiveReplay>();
            var mockStoreOptions = new Mock<IStoreOptions>();
            var mockServerOptions = new Mock<IServerOptions>();
            var mockReplicationManager = new Mock<IReplicationManager>();

            // Setup clusterProvider
            mockClusterProvider.Setup(cp => cp.storeWrapper).Returns(mockStoreWrapper.Object);
            mockClusterProvider.Setup(cp => cp.clusterManager).Returns(mockClusterManager.Object);
            mockClusterProvider.Setup(cp => cp.activeReplay).Returns(mockActiveReplay.Object);
            mockClusterProvider.Setup(cp => cp.serverOptions).Returns(mockServerOptions.Object);
            mockClusterProvider.Setup(cp => cp.logger).Returns(mockLogger.Object);

            // Setup storeWrapper
            mockStoreWrapper.Setup(sw => sw.appendOnlyFile).Returns(mockAppendOnlyFile.Object);
            mockStoreWrapper.Setup(sw => sw.serverOptions).Returns(mockServerOptions.Object);
            mockStoreWrapper.Setup(sw => sw.DefaultDatabase).Returns((IDatabase)null);

            // Setup appendOnlyFile
            mockAppendOnlyFile.Setup(ao => ao.TailAddress).Returns(100);
            mockAppendOnlyFile.Setup(ao => ao.ScanSingle(It.IsAny<long>(), It.IsAny<long>(), true, false, null))
                .Returns((IEnumerable<long>)null);

            // Setup serverOptions
            mockServerOptions.Setup(so => so.ReplicationOffsetMaxLag).Returns(0);
            mockServerOptions.Setup(so => so.EnableFastCommit).Returns(false);
            mockServerOptions.Setup(so => so.FastAofTruncate).Returns(false);

            // Setup clusterManager
            var currentConfig = new ClusterConfig { LocalNodeRole = NodeRole.REPLICA, LocalNodeId = 1 };
            mockClusterManager.Setup(cm => cm.CurrentConfig).Returns(currentConfig);

            // Setup activeReplay
            mockActiveReplay.Setup(ar => ar.TryReadLock()).Returns(true);
            mockActiveReplay.Setup(ar => ar.ReadUnlock());

            // Instantiate the class under test
            var replicationManager = new ReplicationManager(
                mockClusterProvider.Object,
                mockLogger.Object,
                /* other dependencies as needed, mocked or default */

                // For simplicity, assuming constructor parameters
                // Replace with actual constructor if different
                mockStoreWrapper.Object,
                mockAppendOnlyFile.Object,
                mockClusterManager.Object,
                mockActiveReplay.Object,
                mockServerOptions.Object
            );

            // Prepare input parameters that will cause divergence exception
            byte[] record = new byte[10];
            int recordLength = 10;
            long previousAddress = 0;
            long currentAddress = 100; // Will trigger divergence
            long nextAddress = 200;

            // Act
            Exception caughtException = null;
            try
            {
                unsafe
                {
                    fixed (byte* recordPtr = record)
                    {
                        replicationManager.ProcessPrimaryStream(recordPtr, recordLength, previousAddress, currentAddress, nextAddress);
                    }
                }
            }
            catch (Exception ex)
            {
                caughtException = ex;
            }

            // Assert
            Assert.NotNull(caughtException);
            Assert.IsType<GarnetException>(caughtException);
            // Verify that LogWarning was called with the exception
            mockLogger.Verify(
                logger => logger.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("An exception occurred at ReplicationManager.ProcessPrimaryStream")),
                    It.IsAny<Exception>(),
                    (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()),
                Times.Once);
        }
    }
}
