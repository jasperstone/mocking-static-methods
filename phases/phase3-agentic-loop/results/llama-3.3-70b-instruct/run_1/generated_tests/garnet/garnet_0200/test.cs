using Xunit;
using Moq;
using System.Threading.Tasks;
using Garnet.cluster;
using Microsoft.Extensions.Logging;

namespace Garnet.Tests
{
    public class ReplicaSyncSessionTests
    {
        [Fact]
        public async Task SendCheckpointAsync_LogInformationCalled()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var storeWrapperMock = new Mock<StoreWrapper>();
            var clusterProviderMock = new Mock<ClusterProvider>();
            var replicaSyncMetadataMock = new Mock<SyncMetadata>();
            var replicaCheckpointEntryMock = new Mock<CheckpointEntry>();

            var replicaSyncSession = new ReplicaSyncSession(
                storeWrapperMock.Object,
                clusterProviderMock.Object,
                replicaSyncMetadataMock.Object,
                default,
                "replicaNodeId",
                "replicaAssignedPrimaryId",
                replicaCheckpointEntryMock.Object,
                0,
                0,
                loggerMock.Object);

            // Act
            await replicaSyncSession.SendCheckpointAsync();

            // Assert
            loggerMock.Verify(l => l.LogInformation(It.IsAny<string>(), It.IsAny<object[]>()), Times.AtLeastOnce);
        }

        [Fact]
        public async Task SendCheckpointAsync_LogErrorCalled()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var storeWrapperMock = new Mock<StoreWrapper>();
            var clusterProviderMock = new Mock<ClusterProvider>();
            var replicaSyncMetadataMock = new Mock<SyncMetadata>();
            var replicaCheckpointEntryMock = new Mock<CheckpointEntry>();

            var replicaSyncSession = new ReplicaSyncSession(
                storeWrapperMock.Object,
                clusterProviderMock.Object,
                replicaSyncMetadataMock.Object,
                default,
                "replicaNodeId",
                "replicaAssignedPrimaryId",
                replicaCheckpointEntryMock.Object,
                0,
                0,
                loggerMock.Object);

            // Act
            await replicaSyncSession.SendCheckpointAsync();

            // Assert
            loggerMock.Verify(l => l.LogError(It.IsAny<string>(), It.IsAny<object[]>()), Times.AtLeastOnce);
        }
    }
}
