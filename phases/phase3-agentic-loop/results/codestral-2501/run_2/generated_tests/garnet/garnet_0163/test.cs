using System;
using System.Threading.Tasks;
using Garnet.cluster;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

public class ReplicaSyncSessionTests
{
    [Fact]
    public async Task WaitForFlushAsync_ShouldLogError_WhenFlushTaskThrowsException()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<ReplicaSyncSession>>();
        var replicaSyncSession = new ReplicaSyncSession(loggerMock.Object);

        replicaSyncSession.SetFlushTask(Task.FromException<string>(new Exception("Test exception")));

        // Act
        await replicaSyncSession.WaitForFlushAsync();

        // Assert
        loggerMock.Verify(
            x => x.LogError(
                It.IsAny<EventId>(),
                It.IsAny<Exception>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task WaitForSyncCompletionAsync_ShouldLogError_WhenWaitAsyncThrowsException()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<ReplicaSyncSession>>();
        var replicaSyncSession = new ReplicaSyncSession(loggerMock.Object);

        replicaSyncSession.SetStatus(SyncStatus.INPROGRESS);

        // Act
        await replicaSyncSession.WaitForSyncCompletionAsync();

        // Assert
        loggerMock.Verify(
            x => x.LogError(
                It.IsAny<EventId>(),
                It.IsAny<Exception>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.Once);
    }
}
