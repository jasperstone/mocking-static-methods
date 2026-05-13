using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Garnet.cluster.Tests
{
    public class ReplicaSyncSessionTests
    {
        private readonly Mock<ILogger<ReplicaSyncSession>> _mockLogger;
        private readonly ReplicaSyncSession _replicaSyncSession;

        public ReplicaSyncSessionTests()
        {
            _mockLogger = new Mock<ILogger<ReplicaSyncSession>>();
            _replicaSyncSession = new ReplicaSyncSession(_mockLogger.Object);
        }

        [Fact]
        public async Task WaitForFlushAsync_ShouldLogError_WhenFlushTaskThrowsException()
        {
            // Arrange
            var exception = new Exception("Test exception");
            _replicaSyncSession.SetFlushTask(Task.FromException<string>(exception));

            // Act
            await _replicaSyncSession.WaitForFlushAsync();

            // Assert
            _mockLogger.Verify(
                x => x.LogError(
                    It.IsAny<EventId>(),
                    It.IsAny<Exception>(),
                    It.IsAny<string>(),
                    It.IsAny<object[]>()),
                Times.Once);
        }

        [Fact]
        public async Task WaitForSyncCompletionAsync_ShouldLogError_WhenSignalCompletionThrowsException()
        {
            // Arrange
            var exception = new Exception("Test exception");
            _replicaSyncSession.SetStatus(SyncStatus.INPROGRESS);
            _replicaSyncSession.SetSignalCompletion(Task.FromException<bool>(exception));

            // Act
            await _replicaSyncSession.WaitForSyncCompletionAsync();

            // Assert
            _mockLogger.Verify(
                x => x.LogError(
                    It.IsAny<EventId>(),
                    It.IsAny<Exception>(),
                    It.IsAny<string>(),
                    It.IsAny<object[]>()),
                Times.Once);
        }
    }
}
