using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Garnet.cluster; // Assuming this is the correct namespace
using Garnet.client; // Assuming this is the correct namespace for GarnetClientSession
using Garnet.common; // Assuming this is the correct namespace for CheckpointFileType

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

            // Mock the necessary methods and properties
            ckptManagerMock.Setup(m => m.GetLogCheckpointMetadata(It.IsAny<Guid>(), null, true, -1))
                .Returns(Array.Empty<byte>());

            gcsMock.Setup(m => m.ExecuteSendCkptMetadata(It.IsAny<byte[]>(), It.IsAny<int>(), It.IsAny<byte[]>()))
                .ReturnsAsync("OK");

            // Act
            await replicaSyncSession.SendCheckpointMetadataAsync(gcsMock.Object, fileToken, fileType, ckptManagerMock.Object, token);

            // Assert
            loggerMock.Verify(
                m => m.LogInformation(
                    "<Complete sending checkpoint metadata {fileToken} {fileType}",
                    fileToken,
                    fileType),
                Times.Once);
        }
    }
}
