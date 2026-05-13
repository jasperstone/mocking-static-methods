using Xunit;
using Moq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Garnet.cluster;
using Garnet.common;
using System;

namespace ReplicaSyncSessionTests
{
    public class ReplicaSyncSessionTests
    {
        [Fact]
        public async Task SendCheckpointMetadata_LogInformationCalled()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var replicaSyncSession = new ReplicaSyncSession(
                new StoreWrapper(),
                new ClusterProvider(),
                logger: loggerMock.Object);

            var fileToken = Guid.NewGuid();
            var fileType = CheckpointFileType.STORE_SNAPSHOT;

            // Act
            await replicaSyncSession.SendCheckpointAsync();

            // Assert
            loggerMock.Verify(l => l.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => true),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)), Times.AtLeastOnce);
        }

        [Fact]
        public async Task SendCheckpointMetadata_LogErrorCalled()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var replicaSyncSession = new ReplicaSyncSession(
                new StoreWrapper(),
                new ClusterProvider(),
                logger: loggerMock.Object);

            var fileToken = Guid.NewGuid();
            var fileType = CheckpointFileType.STORE_SNAPSHOT;

            // Act
            await replicaSyncSession.SendCheckpointAsync();

            // Assert
            loggerMock.Verify(l => l.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => true),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)), Times.AtLeastOnce);
        }
    }
}
