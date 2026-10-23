using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using Moq;
using Garnet.cluster;
using Microsoft.Extensions.Logging;

namespace Garnet.Tests
{
    public class FailoverSessionLoggingTests
    {
        [Fact]
        public async Task BroadcastConfigAndRequestAttachAsync_ShouldLogWarning_WhenTryTakeOverForPrimaryFails()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<FailoverSession>>();
            var clusterManagerMock = new Mock<ClusterManager>();
            var clusterProviderMock = new Mock<ClusterProvider>();
            var replicationManagerMock = new Mock<ReplicationManager>();
            var storeWrapperMock = new Mock<StoreWrapper>();

            // Setup the clusterProvider mock to return false for TryTakeOverForPrimary
            var clusterProvider = new Mock<ClusterProvider>();
            var clusterManager = new Mock<ClusterManager>();
            var replicationManager = new Mock<ReplicationManager>();
            var storeWrapper = new Mock<StoreWrapper>();

            clusterManager.Setup(cm => cm.TryTakeOverForPrimary()).Returns(false);
            clusterProvider.Setup(cp => cp.clusterManager).Returns(clusterManager.Object);
            clusterProvider.Setup(cp => cp.replicationManager).Returns(replicationManager.Object);
            clusterProvider.Setup(cp => cp.storeWrapper).Returns(storeWrapper.Object);
            // Setup minimal config
            var config = new Config();
            clusterProvider.Setup(cp => cp.CurrentConfig).Returns(config);
            clusterProvider.Setup(cp => cp.ClusterUsername).Returns("user");
            clusterProvider.Setup(cp => cp.ClusterPassword).Returns("pass");
            clusterProvider.Setup(cp => cp.GetEndpointFromNodeId(It.IsAny<string>())).Returns("endpoint");
            clusterProvider.Setup(cp => cp.BumpAndWaitForEpochTransitionAsync()).ReturnsAsync(Task.CompletedTask);
            // Assign the mock to the FailoverSession
            var failoverSession = new FailoverSession
            {
                logger = loggerMock.Object,
                clusterProvider = clusterProvider.Object,
                oldConfig = new Config { LocalNodePrimaryId = "primary", LocalNodeId = "node1" },
                cts = new CancellationTokenSource(),
                failoverTimeout = TimeSpan.FromSeconds(1),
                status = FailoverStatus.NONE
            };

            // Act
            await failoverSession.BroadcastConfigAndRequestAttachAsync("someReplicaId", new byte[] { 1, 2, 3 });

            // Assert
            // Verify that LogWarning was called with the expected message
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("RESP_ERR_GENERIC_CANNOT_TAKEOVER_FROM_PRIMARY")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }
    }
}
