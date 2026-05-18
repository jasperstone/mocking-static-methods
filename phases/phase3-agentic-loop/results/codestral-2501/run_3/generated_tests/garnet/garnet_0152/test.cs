using System;
using System.Threading;
using System.Threading.Tasks;
using Garnet.client;
using Garnet.common;
using Microsoft.Extensions.Logging;
using Moq;
using Tsavorite.core;
using Xunit;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("DynamicProxyGenAssembly2")]

namespace Garnet.cluster.Tests
{
    public class AofSyncTaskInfoTests
    {
        [Fact]
        public async Task ReplicaSyncTaskAsync_LogsInformation()
        {
            // Arrange
            var mockLogger = new Mock<ILogger<AofSyncTaskInfo>>();
            var mockClusterProvider = new Mock<ClusterProvider>();
            var mockAofTaskStore = new Mock<AofTaskStore>();
            var mockGarnetClient = new Mock<GarnetClientSession>();
            var mockIter = new Mock<TsavoriteLogScanSingleIterator>();

            mockClusterProvider.Setup(cp => cp.storeWrapper.appendOnlyFile.ScanSingle(It.IsAny<long>(), It.IsAny<long>(), It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<ILogger>()))
                .Returns(mockIter.Object);

            var aofSyncTaskInfo = new AofSyncTaskInfo(
                mockClusterProvider.Object,
                mockAofTaskStore.Object,
                "localNodeId",
                "remoteNodeId",
                mockGarnetClient.Object,
                0,
                mockLogger.Object);

            // Act
            await aofSyncTaskInfo.ReplicaSyncTaskAsync();

            // Assert
            mockLogger.Verify(
                x => x.Log(
                    It.Is<LogLevel>(l => l == LogLevel.Information),
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Starting ReplicationManager.ReplicaSyncTask for remote node remoteNodeId starting from address 0")),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
                Times.Once);
        }

        [Fact]
        public void Throttle_ThrowsException_WhenNotConnected()
        {
            // Arrange
            var mockGarnetClient = new Mock<GarnetClientSession>();
            mockGarnetClient.Setup(gc => gc.IsConnected).Returns(false);

            var aofSyncTaskInfo = new AofSyncTaskInfo(
                null,
                null,
                null,
                null,
                mockGarnetClient.Object,
                0,
                null);

            // Act & Assert
            Assert.Throws<GarnetException>(() => aofSyncTaskInfo.Throttle());
        }
    }
}
