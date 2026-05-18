using System;
using System.Threading;
using Garnet.cluster;
using Garnet.common;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Garnet.cluster.Tests
{
    public class ReplicationReplicaAofSyncTests
    {
        [Fact]
        public void ProcessPrimaryStream_ExceptionLogged_WhenExceptionOccurs()
        {
            // Arrange
            var mockLogger = new Mock<ILogger>();
            var mockClusterProvider = new Mock<ClusterProvider>();
            var mockStoreWrapper = new Mock<StoreWrapper>();
            var mockAppendOnlyFile = new Mock<AppendOnlyFile>();
            var mockServerOptions = new Mock<ServerOptions>();
            var mockClusterManager = new Mock<ClusterManager>();
            var mockReplicationManager = new Mock<ReplicationManager>(mockClusterProvider.Object, mockLogger.Object);

            mockClusterProvider.Setup(cp => cp.storeWrapper).Returns(mockStoreWrapper.Object);
            mockClusterProvider.Setup(cp => cp.serverOptions).Returns(mockServerOptions.Object);
            mockClusterProvider.Setup(cp => cp.clusterManager).Returns(mockClusterManager.Object);
            mockClusterProvider.Setup(cp => cp.replicationManager).Returns(mockReplicationManager.Object);

            mockStoreWrapper.Setup(sw => sw.appendOnlyFile).Returns(mockAppendOnlyFile.Object);
            mockStoreWrapper.Setup(sw => sw.serverOptions).Returns(mockServerOptions.Object);

            mockServerOptions.Setup(so => so.ReplicationOffsetMaxLag).Returns(0);

            var replicaAofSync = new ReplicationReplicaAofSync(mockClusterProvider.Object, mockLogger.Object);

            byte[] record = new byte[10];
            int recordLength = record.Length;
            long previousAddress = 0;
            long currentAddress = 10;
            long nextAddress = 20;

            // Act
            Action act = () => replicaAofSync.ProcessPrimaryStream(record, recordLength, previousAddress, currentAddress, nextAddress);

            // Assert
            act.Should().Throw<GarnetException>();
            mockLogger.Verify(
                x => x.Log(
                    It.Is<LogLevel>(l => l == LogLevel.Warning),
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => true),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
                Times.Once);
        }
    }
}
