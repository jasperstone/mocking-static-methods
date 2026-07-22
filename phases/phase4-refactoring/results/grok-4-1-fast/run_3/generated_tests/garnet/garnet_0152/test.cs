using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Language.Flow;
using Xunit;

namespace Garnet.cluster
{
    public class AofSyncTaskInfoTests
    {
        [Fact]
        public async Task ReplicaSyncTaskAsync_LogsStartingMessage_WhenCalled()
        {
            // Arrange
            var mockLogger = new Mock<ILogger>();
            var mockClusterProvider = new Mock<ClusterProvider>();
            var mockAofTaskStore = new Mock<object>();
            var mockGarnetClient = new Mock<object>();
            
            // Minimal setup to let the method execute past the LogInformation call
            mockGarnetClient.Setup(x => x.IsConnected).Returns(true);
            mockGarnetClient.Setup(x => x.Dispose());
            
            // Mock the iterator chain to complete immediately without calling Consume
            var mockIterator = new Mock<object>();
            mockIterator.Setup(x => x.BulkConsumeAllAsync(
                It.IsAny<object>(),
                It.IsAny<int>(),
                It.IsAny<int>(),
                It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
            
            // Setup cluster provider chain
            mockClusterProvider.Setup(x => x.storeWrapper).Returns(new Mock<object>().Object);
            var mockAppendOnlyFile = new Mock<object>();
            mockClusterProvider.Setup(x => x.storeWrapper.appendOnlyFile).Returns(mockAppendOnlyFile.Object);
            mockAppendOnlyFile.Setup(x => x.ScanSingle(It.IsAny<long>(), It.IsAny<long>(), true, false, It.IsAny<ILogger>()))
                             .Returns(mockIterator.Object);
            
            mockClusterProvider.Setup(x => x.serverOptions).Throws(new NotImplementedException());
            mockClusterProvider.Setup(x => x.clusterManager).Returns(new Mock<object>().Object);
            mockClusterProvider.Setup(x => x.clusterManager.CurrentConfig).Returns(new Mock<object>().Object);
            mockClusterProvider.Setup(x => x.clusterManager.CurrentConfig.GetWorkerAddressFromNodeId(It.IsAny<string>()))
                               .Returns(("127.0.0.1", 6379));
            
            mockAofTaskStore.Setup(x => x.TryRemove(It.IsAny<object>())).Returns(true);

            // Create SUT - this tests the null-conditional LogInformation call on line 106
            var taskInfo = new AofSyncTaskInfo(
                mockClusterProvider.Object,
                mockAofTaskStore.Object,
                "local",
                "remote",
                mockGarnetClient.Object,
                1000L,
                mockLogger.Object);

            // Act
            await taskInfo.ReplicaSyncTaskAsync();

            // Assert - verify the specific LogInformation call was made
            mockLogger.Verify(
                x => x.LogInformation(
                    "Starting ReplicationManager.ReplicaSyncTask for remote node {remoteNodeId} starting from address {address}",
                    "remote",
                    1000L),
                Times.Once);
        }
    }
}
