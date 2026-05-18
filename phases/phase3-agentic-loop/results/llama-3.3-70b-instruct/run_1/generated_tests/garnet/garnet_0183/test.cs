using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using Garnet.cluster;

namespace GarnetTests
{
    public class ReplicaSyncSessionTests
    {
        [Fact]
        public async Task SendCheckpointAsync_LogInformationCalled()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var replicaSyncSession = new PublicReplicaSyncSession(
                loggerMock.Object);

            // Act
            await replicaSyncSession.SendCheckpointAsync();

            // Assert
            loggerMock.Verify(l => l.LogInformation(It.IsAny<string>(), It.IsAny<object[]>()), Times.AtLeastOnce);
        }

        [Fact]
        public async Task SendCheckpointAsync_LogInformationCalledWithCorrectMessage()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var replicaSyncSession = new PublicReplicaSyncSession(
                loggerMock.Object);

            // Act
            await replicaSyncSession.SendCheckpointAsync();

            // Assert
            loggerMock.Verify(l => l.LogInformation("Checkpoint search completed"), Times.Once);
        }
    }

    public class PublicReplicaSyncSession
    {
        private readonly ReplicaSyncSession _replicaSyncSession;

        public PublicReplicaSyncSession(ILogger logger)
        {
            _replicaSyncSession = new ReplicaSyncSession(
                new StoreWrapper(),
                new ClusterProvider(),
                replicaNodeId: "replicaNodeId",
                replicaCheckpointEntry: new CheckpointEntry(),
                logger: logger);
        }

        public async Task SendCheckpointAsync()
        {
            await _replicaSyncSession.SendCheckpointAsync();
        }
    }
}
