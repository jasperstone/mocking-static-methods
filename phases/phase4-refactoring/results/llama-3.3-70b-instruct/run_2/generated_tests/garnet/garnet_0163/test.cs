using Xunit;
using Moq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Garnet.cluster
{
    public class ReplicaSyncSessionTests
    {
        [Fact]
        public async Task WaitForFlushAsync_LogsErrorOnException()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var replicaSyncSession = new ReplicaSyncSession(loggerMock.Object);
            replicaSyncSession.flushTask = Task.FromException(new Exception("Test exception"));

            // Act
            await replicaSyncSession.WaitForFlushAsync();

            // Assert
            loggerMock.Verify(l => l.LogError(It.IsAny<Exception>(), "{method}", "WaitForFlushAsync"), Times.Once);
        }

        [Fact]
        public async Task WaitForSyncCompletionAsync_LogsErrorOnException()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var replicaSyncSession = new ReplicaSyncSession(loggerMock.Object);
            replicaSyncSession.signalCompletion = new SemaphoreSlim(0);

            // Act and Assert
            await Assert.ThrowsAsync<Exception>(() => replicaSyncSession.WaitForSyncCompletionAsync());
            loggerMock.Verify(l => l.LogError(It.IsAny<Exception>(), "{method} failed waiting for sync", "WaitForSyncCompletionAsync"), Times.Once);
        }
    }
}
