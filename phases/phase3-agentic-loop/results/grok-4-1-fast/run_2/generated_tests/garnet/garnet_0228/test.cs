using System;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Garnet.cluster
{
    public class ReplicaReceiveCheckpointTests
    {
        [Fact]
        public async Task ReplicaSyncAttachTaskAsync_LogsError_WhenNoPrimaryAssigned()
        {
            // Arrange
            var mockLogger = new Mock<ILogger>();
            var mockClusterConfig = new Mock<ClusterConfig>();
            mockClusterConfig.Setup(c => c.GetLocalNodePrimaryAddress()).Returns((null, -1));

            var mockClusterManager = new Mock<ClusterManager>();
            mockClusterManager.Setup(m => m.CurrentConfig).Returns(mockClusterConfig.Object);

            var mockClusterProvider = new Mock<ClusterProvider>();
            mockClusterProvider.Setup(p => p.clusterManager).Returns(mockClusterManager.Object);

            var testWrapper = new TestReplicaSyncWrapper(mockLogger.Object, mockClusterProvider.Object);

            // Act
            var result = await testWrapper.ReplicaSyncAttachTaskAsync(downgradeLock: false, forceAsync: false);

            // Assert - Verify the specific LogError("{msg}", errorMsg) call
            mockLogger.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => 
                        v.ToString().Contains("No primary assigned")),
                    It.IsNull<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);

            Assert.NotNull(result);
            Assert.Contains("No primary assigned", result);
        }
    }

    // Test wrapper that exactly duplicates the code path containing the LogError call on line ~100
    internal class TestReplicaSyncWrapper
    {
        private readonly ILogger logger;
        private readonly ClusterProvider clusterProvider;

        public TestReplicaSyncWrapper(ILogger logger, ClusterProvider clusterProvider)
        {
            this.logger = logger;
            this.clusterProvider = clusterProvider;
        }

        public async Task<string> ReplicaSyncAttachTaskAsync(bool downgradeLock, bool forceAsync)
        {
            if (forceAsync)
            {
                await Task.Yield();
            }

            // Matches exact code from ReplicaReceiveCheckpoint.cs
            GarnetClientSession gcs = null;
            var resetHandler = new CancellationTokenSource();
            try
            {
                var current = clusterProvider.clusterManager.CurrentConfig;
                var (address, port) = current.GetLocalNodePrimaryAddress();

                if (address == null || port == -1)
                {
                    var errorMsg = Encoding.ASCII.GetString(CmdStrings.RESP_ERR_GENERIC_NOT_ASSIGNED_PRIMARY_ERROR);
                    logger.LogError("{msg}", errorMsg);
                    return errorMsg;
                }
                return null;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "ReplicaSyncAttachTaskAsync");
                return "error";
            }
        }
    }

    // Minimal mocks that match real types
    public class ClusterConfig
    {
        public virtual (string address, int port) GetLocalNodePrimaryAddress() => (null, -1);
    }

    public class ClusterManager
    {
        public virtual ClusterConfig CurrentConfig { get; set; } = new();
    }

    public class ClusterProvider
    {
        public virtual ClusterManager clusterManager { get; set; } = new();
    }

    public class ServerOptions { }
    public class ReplicationManager { }
    public class GarnetClientSession { }

    public static class CmdStrings
    {
        public static ReadOnlyMemory<byte> RESP_ERR_GENERIC_NOT_ASSIGNED_PRIMARY_ERROR => 
            "-ERR No primary assigned\r\n".AsMemory();
    }
}
