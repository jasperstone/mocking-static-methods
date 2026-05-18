using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Garnet.cluster; // Assuming ReplicaSyncSession is in this namespace
using Garnet.client; // Assuming GarnetClientSession is in this namespace
using Garnet.common; // Assuming CheckpointFileType is in this namespace
using Garnet.server; // Assuming StoreWrapper and ICheckpointManager are in this namespace

namespace Garnet.cluster.Tests
{
    public class ReplicaSyncSessionTests
    {
        [Fact]
        public async Task LogInformation_ShouldBeCalled_WhenSendingCheckpointMetadataCompletes()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var storeWrapperMock = new Mock<StoreWrapper>();
            var clusterProviderMock = new Mock<ClusterProvider>();
            var ckptManagerMock = new Mock<ICheckpointManager>();
            var gcsMock = new Mock<GarnetClientSession>();

            var fileToken = Guid.NewGuid();
            var fileType = CheckpointFileType.STORE_SNAPSHOT;
            var token = new CancellationToken();

            var replicaSyncSession = new ReplicaSyncSession(
                storeWrapperMock.Object,
                clusterProviderMock.Object,
                logger: loggerMock.Object);

            // Setup mock behavior
            gcsMock.Setup(g => g.ExecuteSendCkptMetadata(It.IsAny<byte[]>(), It.IsAny<int>(), It.IsAny<byte[]>()))
                .ReturnsAsync("OK");

            // Act
            await replicaSyncSession.SendCheckpointMetadataAsync(gcsMock.Object, fileToken, fileType, ckptManagerMock.Object, token);

            // Assert
            loggerMock.Verify(
                l => l.LogInformation(
                    "<Complete sending checkpoint metadata {fileToken} {fileType}",
                    fileToken,
                    fileType),
                Times.Once);
        }
    }
}
