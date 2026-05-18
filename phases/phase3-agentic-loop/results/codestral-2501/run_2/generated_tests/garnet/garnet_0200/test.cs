using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using Garnet.cluster;
using Garnet.common;
using System.Threading.Tasks;
using System.Threading;
using System;
using System.Net;
using System.Reflection;

namespace Garnet.Tests
{
    public class ReplicaSyncSessionTests
    {
        [Fact]
        public async Task SendCheckpointAsync_LogsInformation_WhenMetadataSentSuccessfully()
        {
            // Arrange
            var mockLogger = new Mock<ILogger<ReplicaSyncSession>>();
            var mockStoreWrapper = new Mock<StoreWrapper>();
            var mockClusterProvider = new Mock<ClusterProvider>();
            var mockGarnetClientSession = new Mock<GarnetClientSession>(new IPEndPoint(IPAddress.Loopback, 0), null, null, null, null, null, mockLogger.Object);
            var mockCkptManager = new Mock<ReplicationLogCheckpointManager>();

            mockStoreWrapper.Setup(sw => sw.serverOptions.ReplicaSyncTimeout).Returns(TimeSpan.FromSeconds(10));
            mockClusterProvider.Setup(cp => cp.GetReplicationLogCheckpointManager(StoreType.Main)).Returns(mockCkptManager.Object);
            mockClusterProvider.Setup(cp => cp.GetReplicationLogCheckpointManager(StoreType.Object)).Returns(mockCkptManager.Object);
            mockClusterProvider.Setup(cp => cp.clusterManager.CurrentConfig.GetWorkerAddressFromNodeId(It.IsAny<string>())).Returns((IPAddress.Loopback.ToString(), 12345));
            mockGarnetClientSession.Setup(gcs => gcs.ExecuteSendCkptMetadata(It.IsAny<byte[]>(), It.IsAny<int>(), It.IsAny<byte[]>())).ReturnsAsync("OK");

            var replicaSyncSession = (ReplicaSyncSession)Activator.CreateInstance(typeof(ReplicaSyncSession), BindingFlags.NonPublic | BindingFlags.Instance, null,
                new object[] { mockStoreWrapper.Object, mockClusterProvider.Object, null, default(CancellationToken), null, null, null, 0, 0, mockLogger.Object }, null);

            // Act
            await replicaSyncSession.SendCheckpointAsync();

            // Assert
            mockLogger.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("<Complete sending checkpoint metadata")),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
                Times.AtLeastOnce);
        }
    }
}
