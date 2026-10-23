using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using Moq;
using Garnet.cluster;
using Garnet.client;
using Microsoft.Extensions.Logging;

namespace Garnet.Tests
{
    // Minimal stub classes to satisfy compilation
    internal class StoreWrapper
    {
        public AppendOnlyFile appendOnlyFile = new AppendOnlyFile();
    }

    internal class AppendOnlyFile
    {
        public TsavoriteLogScanSingleIterator ScanSingle(long start, long end, bool scanUncommitted, bool recover, ILogger logger)
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

    internal class ClusterManager
    {
        public ClusterConfig CurrentConfig { get; } = new ClusterConfig();
    }

    internal class ClusterConfig
    {
        public (string, int) GetWorkerAddressFromNodeId(string nodeId)
        {
            return ("127.0.0.1", 6379);
        }
    }

    internal class ClusterProvider
    {
        public StoreWrapper storeWrapper = new StoreWrapper();
        public ServerOptions serverOptions = new ServerOptions();
        public ClusterManager clusterManager = new ClusterManager();
    }

    internal class ServerOptions
    {
        public int ReplicaSyncDelayMs { get; set; } = 10;
    }

    internal class AofTaskStore
    {
        public bool TryRemove(AofSyncTaskInfo task) => true;
    }

    internal class GarnetClientSession
    {
        public bool IsConnected => true;
        public void Connect() { }
        public void Dispose() { }
        public void ExecuteClusterAppendLog(string localNodeId, long previousAddress, long currentAddress, long nextAddress, long payloadPtr, int payloadLength) { }
        public void CompletePending(bool flush) { }
        public void Throttle() { }
    }

    public class AofSyncTaskInfoTests
    {
        [Fact]
        public async Task ReplicaSyncTaskAsync_LogsStartingMessageAndEndsGracefully()
        {
            // Arrange
            var mockLogger = new Mock<ILogger>();
            var mockGarnetClient = new Mock<GarnetClientSession>();
            var mockClusterProvider = new ClusterProvider();
            var mockAofTaskStore = new AofTaskStore();

            // Setup garnetClient to be connected
            mockGarnetClient.Setup(c => c.IsConnected).Returns(true);
            mockGarnetClient.Setup(c => c.Dispose());

            var taskInfo = new AofSyncTaskInfo(
                clusterProvider: mockClusterProvider,
                aofTaskStore: mockAofTaskStore,
                localNodeId: "local",
                remoteNodeId: "remote",
                garnetClient: mockGarnetClient.Object,
                startAddress: 0,
                logger: mockLogger.Object);

            // Act
            await taskInfo.ReplicaSyncTaskAsync();

            // Assert
            mockLogger.Verify(
                logger => logger.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Starting ReplicationManager.ReplicaSyncTask for remote node remote starting from address 0")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }
    }
}
