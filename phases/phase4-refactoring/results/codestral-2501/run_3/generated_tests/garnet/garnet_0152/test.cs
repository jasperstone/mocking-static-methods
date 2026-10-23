using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using Garnet.cluster;
using Garnet.client;
using System.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using Garnet.common;
using Garnet.server;

public class AofSyncTaskInfoTests
{
    [Fact]
    public async Task ReplicaSyncTaskAsync_LogsInformation()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<AofSyncTaskInfo>>();
        var mockGarnetClient = new Mock<GarnetClientSession>();
        var mockClusterProvider = new Mock<ClusterProvider>();
        var mockAofTaskStore = new Mock<AofTaskStore>();
        var mockStoreWrapper = new Mock<StoreWrapper>();
        var mockAppendOnlyFile = new Mock<AppendOnlyFile>();
        var mockIter = new Mock<TsavoriteLogScanSingleIterator>();

        mockClusterProvider.Setup(cp => cp.storeWrapper).Returns(mockStoreWrapper.Object);
        mockStoreWrapper.Setup(sw => sw.appendOnlyFile).Returns(mockAppendOnlyFile.Object);
        mockAppendOnlyFile.Setup(aof => aof.ScanSingle(It.IsAny<long>(), It.IsAny<long>(), It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<ILogger>())).Returns(mockIter.Object);

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
            logger => logger.Log(
                It.Is<LogLevel>(logLevel => logLevel == LogLevel.Information),
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => true),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
            Times.Once);
    }

    internal class StoreWrapper
    {
        public AppendOnlyFile appendOnlyFile { get; set; }
    }

    internal class AppendOnlyFile
    {
        public TsavoriteLogScanSingleIterator ScanSingle(long startAddress, long endAddress, bool scanUncommitted, bool recover, ILogger logger)
        {
            return new TsavoriteLogScanSingleIterator();
        }
    }

    internal class TsavoriteLogScanSingleIterator
    {
        public Task BulkConsumeAllAsync(IBulkLogEntryConsumer consumer, int delayMs, int maxChunkSize, CancellationToken token)
        {
            return Task.CompletedTask;
        }
    }

    internal interface IBulkLogEntryConsumer
    {
        void Consume(byte* payloadPtr, int payloadLength, long currentAddress, long nextAddress, bool isProtected);
    }
}
