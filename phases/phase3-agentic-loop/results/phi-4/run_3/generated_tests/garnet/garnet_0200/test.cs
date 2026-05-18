using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Garnet.cluster; // Namespace for ReplicaSyncSession
using Garnet.client; // Namespace for GarnetClientSession
using Garnet.common; // Namespace for CheckpointFileType
using Garnet.server; // Namespace for StoreWrapper and ICheckpointManager

namespace Garnet.cluster.Tests
{
    public class ReplicaSyncSessionTests
    {
        [Fact]
        public async Task LogInformation_ShouldBeCalled_WithCorrectParameters()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var storeWrapper = new Mock<StoreWrapper>();
            var clusterProvider = new Mock<ClusterProvider>();
            var gcs = new Mock<GarnetClientSession>();
            var ckptManager = new Mock<ICheckpointManager>();

            var fileToken = Guid.NewGuid();
            var fileType = CheckpointFileType.STORE_SNAPSHOT; // Ensure this is accessible
            var token = new CancellationToken();

            var replicaSyncSession = new ReplicaSyncSession(
                storeWrapper.Object,
                clusterProvider.Object,
                logger: loggerMock.Object);

            // Act
            await replicaSyncSession.SendCheckpointMetadataAsync(gcs.Object, fileToken, fileType, ckptManager.Object, token);

            // Assert
            loggerMock.Verify(
                x => x.LogInformation(
                    "<Complete sending checkpoint metadata {fileToken} {fileType}",
                    fileToken,
                    fileType),
                Times.Once);
        }
    }
}
