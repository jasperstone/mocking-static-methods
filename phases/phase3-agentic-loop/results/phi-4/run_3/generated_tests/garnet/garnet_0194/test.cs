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
        public async Task AcquireCheckpointEntry_LogsInformationOnIteration()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var storeWrapperMock = new Mock<StoreWrapper>();
            storeWrapperMock.Setup(s => s.lastSaveTime).Returns(1234567890L); // Mock lastSaveTime

            var clusterProviderMock = new Mock<ClusterProvider>();
            var replicaSyncMetadata = new SyncMetadata();
            var token = CancellationToken.None;
            var replicaNodeId = "replicaNodeId";
            var replicaAssignedPrimaryId = "replicaAssignedPrimaryId";
            var replicaCheckpointEntry = new CheckpointEntry();
            var replicaAofBeginAddress = 0L;
            var replicaAofTailAddress = 0L;

            var session = new ReplicaSyncSession(
                storeWrapperMock.Object,
                clusterProviderMock.Object,
                replicaSyncMetadata,
                token,
                replicaNodeId,
                replicaAssignedPrimaryId,
                replicaCheckpointEntry,
                replicaAofBeginAddress,
                replicaAofTailAddress,
                loggerMock.Object);

            // Act
            await session.AcquireCheckpointEntryAsync();

            // Assert
            loggerMock.Verify(
                x => x.LogInformation(
                    It.Is<string>(s => s == "AcquireCheckpointEntry iteration {iteration}"),
                    It.IsAny<int>()),
                Times.AtLeastOnce);
        }
    }
}
