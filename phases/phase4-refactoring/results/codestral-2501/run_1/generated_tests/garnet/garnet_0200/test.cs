using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using Garnet.client;
using Garnet.common;
using Garnet.server;
using Tsavorite.core;

namespace Garnet.cluster.Tests
{
    public class ReplicaSyncSessionTests
    {
        [Fact]
        public async Task SendCheckpointAsync_LogsInformation_WhenMetadataSentSuccessfully()
        {
            // Arrange
            var mockLogger = new Mock<ILogger>();
            var mockGcs = new Mock<GarnetClientSession>();
            var mockCkptManager = new Mock<ReplicationLogCheckpointManager>();
            var mockClusterProvider = new Mock<ClusterProvider>();
            var mockStoreWrapper = new Mock<StoreWrapper>();

            mockClusterProvider.Setup(cp => cp.GetReplicationLogCheckpointManager(It.IsAny<StoreType>())).Returns(mockCkptManager.Object);
            mockStoreWrapper.Setup(sw => sw.serverOptions).Returns(new ServerOptions { ReplicaSyncTimeout = TimeSpan.FromSeconds(10) });

            var replicaSyncSession = new ReplicaSyncSession(
                mockStoreWrapper.Object,
                mockClusterProvider.Object,
                logger: mockLogger.Object);

            var fileToken = Guid.NewGuid();
            var fileType = CheckpointFileType.STORE_SNAPSHOT;

            mockGcs.Setup(gcs => gcs.ExecuteSendCkptMetadata(It.IsAny<byte[]>(), It.IsAny<int>(), It.IsAny<byte[]>()))
                   .ReturnsAsync("OK");

            // Act
            await replicaSyncSession.SendCheckpointAsync();

            // Assert
            mockLogger.Verify(
                logger => logger.LogInformation(
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>(),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.AtLeastOnce);
        }
    }
}
