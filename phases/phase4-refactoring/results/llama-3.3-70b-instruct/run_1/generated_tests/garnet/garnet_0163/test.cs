using Xunit;
using Moq;
using System.Threading.Tasks;
using Garnet.cluster;
using Microsoft.Extensions.Logging;

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
        replicaSyncSession.token = new CancellationTokenSource().Token;

        // Act and Assert
        await Assert.ThrowsAsync<OperationCanceledException>(() => replicaSyncSession.WaitForSyncCompletionAsync());
        loggerMock.Verify(l => l.LogError(It.IsAny<Exception>(), "{method} failed waiting for sync", "WaitForSyncCompletionAsync"), Times.Once);
    }
}
