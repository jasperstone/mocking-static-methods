using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using Garnet.cluster;
using System;
using Garnet.common;
using Garnet.server;

namespace Garnet.cluster.Tests
{
    public class ReplicationReplicaAofSyncTests
    {
        [Fact]
        public void ProcessPrimaryStream_ExceptionLogged()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<ReplicationManager>>();
            var clusterProviderMock = new Mock<ClusterProvider>();
            var storeWrapperMock = new Mock<StoreWrapper>();
            var appendOnlyFileMock = new Mock<AppendOnlyFile>();
            var serverOptionsMock = new Mock<ServerOptions>();
            var clusterManagerMock = new Mock<ClusterManager>();
            var replicationManagerMock = new Mock<ReplicationManager>(clusterProviderMock.Object, loggerMock.Object);

            clusterProviderMock.Setup(cp => cp.storeWrapper).Returns(storeWrapperMock.Object);
            storeWrapperMock.Setup(sw => sw.appendOnlyFile).Returns(appendOnlyFileMock.Object);
            storeWrapperMock.Setup(sw => sw.serverOptions).Returns(serverOptionsMock.Object);
            clusterProviderMock.Setup(cp => cp.clusterManager).Returns(clusterManagerMock.Object);
            clusterProviderMock.Setup(cp => cp.replicationManager).Returns(replicationManagerMock.Object);

            var replicationManager = new ReplicationManager(clusterProviderMock.Object, loggerMock.Object);

            var record = new byte[10];
            var recordLength = record.Length;
            var previousAddress = 0L;
            var currentAddress = 10L;
            var nextAddress = 20L;

            // Act
            Action act = () => replicationManager.ProcessPrimaryStream(record, recordLength, previousAddress, currentAddress, nextAddress);

            // Assert
            Assert.Throws<GarnetException>(act);
            loggerMock.Verify(
                x => x.Log(
                    It.IsAny<LogLevel>(),
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("An exception occurred at ReplicationManager.ProcessPrimaryStream")),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
                Times.Once);
        }
    }
}
