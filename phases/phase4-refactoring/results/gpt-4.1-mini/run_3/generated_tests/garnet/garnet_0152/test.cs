using System;
using System.Threading.Tasks;
using Garnet.cluster;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Garnet.Tests
{
    public class AofSyncTaskInfoTests
    {
        [Fact]
        public async Task ReplicaSyncTaskAsync_LogsInformationOnStart()
        {
            // Arrange
            var clusterProviderMock = new Mock<ClusterProvider>(MockBehavior.Strict);
            var aofTaskStoreMock = new Mock<AofTaskStore>(MockBehavior.Strict);
            var garnetClientMock = new Mock<Garnet.client.GarnetClientSession>(MockBehavior.Strict);
            var loggerMock = new Mock<ILogger>();

            string localNodeId = "localNode";
            string remoteNodeId = "remoteNode";
            long startAddress = 123;

            // Setup clusterProvider to allow ScanSingle call and serverOptions
            var appendOnlyFileMock = new Mock<Tsavorite.core.AppendOnlyFile>(MockBehavior.Strict);
            var iterMock = new Mock<Tsavorite.core.TsavoriteLogScanSingleIterator>(MockBehavior.Strict);
            var serverOptionsMock = new Mock<ClusterProvider.ServerOptions>(MockBehavior.Strict);
            serverOptionsMock.SetupGet(s => s.ReplicaSyncDelayMs).Returns(10);

            clusterProviderMock.SetupGet(c => c.storeWrapper).Returns(new StoreWrapperMock(appendOnlyFileMock.Object));
            clusterProviderMock.SetupGet(c => c.serverOptions).Returns(serverOptionsMock.Object);
            clusterProviderMock.SetupGet(c => c.clusterManager).Returns(new ClusterManagerMock());

            appendOnlyFileMock
                .Setup(a => a.ScanSingle(startAddress, long.MaxValue, true, false, loggerMock.Object))
                .Returns(iterMock.Object);

            iterMock
                .Setup(i => i.BulkConsumeAllAsync(It.IsAny<IBulkLogEntryConsumer>(), 10, 1 << 20, It.IsAny<System.Threading.CancellationToken>()))
                .Returns(Task.CompletedTask);

            garnetClientMock.SetupGet(g => g.IsConnected).Returns(true);
            garnetClientMock.Setup(g => g.Dispose());

            aofTaskStoreMock.Setup(a => a.TryRemove(It.IsAny<AofSyncTaskInfo>())).Returns(true);

            var aofSyncTaskInfo = new AofSyncTaskInfo(
                clusterProviderMock.Object,
                aofTaskStoreMock.Object,
                localNodeId,
                remoteNodeId,
                garnetClientMock.Object,
                startAddress,
                loggerMock.Object);

            // Act
            await aofSyncTaskInfo.ReplicaSyncTaskAsync();

            // Assert
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Starting ReplicationManager.ReplicaSyncTask")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        // Mocks for nested dependencies
        private class StoreWrapperMock
        {
            public Tsavorite.core.AppendOnlyFile appendOnlyFile;
            public StoreWrapperMock(Tsavorite.core.AppendOnlyFile appendOnlyFile)
            {
                this.appendOnlyFile = appendOnlyFile;
            }
        }

        private class ClusterManagerMock
        {
            public CurrentConfigMock CurrentConfig { get; } = new CurrentConfigMock();
        }

        private class CurrentConfigMock
        {
            public (string address, int port) GetWorkerAddressFromNodeId(string nodeId)
            {
                return ("127.0.0.1", 1234);
            }
        }
    }
}
