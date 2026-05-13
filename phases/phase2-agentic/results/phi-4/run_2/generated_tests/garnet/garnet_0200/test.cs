using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Garnet.cluster.Tests
{
    public class ReplicaSyncSessionTests
    {
        [Fact]
        public async Task SendCheckpointMetadata_LogsCompleteSendingCheckpointMetadata()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var ckptManagerMock = new Mock<ICheckpointManager>();
            var clusterProviderMock = new Mock<ClusterProvider>();
            var storeWrapperMock = new Mock<StoreWrapper>();
            var gcsMock = new Mock<GarnetClientSession>();

            // Setup mock behavior for ckptManagerMock and gcsMock as needed
            // For example, setup ckptManagerMock to return specific metadata
            ckptManagerMock.Setup(m => m.GetLogCheckpointMetadata(It.IsAny<Guid>(), null, true, -1))
                .Returns(Array.Empty<byte>());

            ckptManagerMock.Setup(m => m.GetIndexCheckpointMetadata(It.IsAny<Guid>()))
                .Returns(Array.Empty<byte>());

            gcsMock.Setup(g => g.ExecuteSendCkptMetadata(It.IsAny<byte[]>(), It.IsAny<int>(), It.IsAny<byte[]>()))
                .ReturnsAsync("OK");

            var session = new ReplicaSyncSession(
                storeWrapperMock.Object,
                clusterProviderMock.Object,
                logger: loggerMock.Object);

            // Act
            await session.SendCheckpointAsync();

            // Assert
            loggerMock.Verify(
                x => x.LogInformation(
                    "<Complete sending checkpoint metadata {fileToken} {fileType}",
                    It.IsAny<Guid>(),
                    It.IsAny<CheckpointFileType>()),
                Times.AtLeastOnce);
        }
    }
}
