using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Garnet.cluster;
using Garnet.client;
using Tsavorite.core;

namespace Garnet.cluster.Tests
{
    public class AofSyncTaskInfoTests
    {
        [Fact]
        public async Task ReplicaSyncTaskAsync_LogsInformationOnStart()
        {
            // Arrange
            var clusterProviderMock = new Mock<ClusterProvider>(MockBehavior.Strict, null, null, null);
            var aofTaskStoreMock = new Mock<AofTaskStore>();
            var garnetClientMock = new Mock<GarnetClientSession>();
            var loggerMock = new Mock<ILogger>();

            string localNodeId = "localNode";
            string remoteNodeId = "remoteNode";
            long startAddress = 123;

            // Setup clusterProvider to return mocks for required properties
            var storeWrapperMock = new Mock<StoreWrapper>();
            var appendOnlyFileMock = new Mock<AppendOnlyFile>();
            var iterMock = new Mock<TsavoriteLogScanSingleIterator>();

            clusterProviderMock.SetupGet(c => c.storeWrapper).Returns(storeWrapperMock.Object);
            storeWrapperMock.SetupGet(s => s.appendOnlyFile).Returns(appendOnlyFileMock.Object);
            appendOnlyFileMock.Setup(a => a.ScanSingle(startAddress, long.MaxValue, true, false, loggerMock.Object))
                .Returns(iterMock.Object);

            var serverOptionsMock = new Mock<ServerOptions>();
            serverOptionsMock.SetupGet(o => o.ReplicaSyncDelayMs).Returns(10);
            clusterProviderMock.SetupGet(c => c.serverOptions).Returns(serverOptionsMock.Object);

            var clusterManagerMock = new Mock<ClusterManager>();
            var currentConfigMock = new Mock<CurrentConfig>();
            currentConfigMock.Setup(c => c.GetWorkerAddressFromNodeId(remoteNodeId)).Returns(("127.0.0.1", 1234));
            clusterManagerMock.SetupGet(c => c.CurrentConfig).Returns(currentConfigMock.Object);
            clusterProviderMock.SetupGet(c => c.clusterManager).Returns(clusterManagerMock.Object);

            garnetClientMock.SetupGet(g => g.IsConnected).Returns(false);
            garnetClientMock.Setup(g => g.Connect());
            iterMock.Setup(i => i.BulkConsumeAllAsync(It.IsAny<IBulkLogEntryConsumer>(), 10, 1 << 20, It.IsAny<System.Threading.CancellationToken>()))
                .Returns(Task.CompletedTask);
            garnetClientMock.Setup(g => g.Dispose());

            aofTaskStoreMock.Setup(s => s.TryRemove(It.IsAny<AofSyncTaskInfo>())).Returns(true);

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
    }
}
