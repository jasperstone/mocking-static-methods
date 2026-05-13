using System;
using System.Threading.Tasks;
using Garnet.cluster;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

public class ReplicaSyncSessionTests
{
    [Fact]
    public async Task WaitForFlushAsync_LogsError_WhenFlushTaskThrowsException()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<ReplicaSyncSession>>();
        var replicaSyncSession = new ReplicaSyncSession
        {
            flushTask = Task.FromException(new Exception("Test exception")),
            logger = loggerMock.Object
        };

        // Act
        await replicaSyncSession.WaitForFlushAsync();

        // Assert
        loggerMock.Verify(
            x => x.LogError(
                It.IsAny<Exception>(),
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains(nameof(replicaSyncSession.WaitForFlushAsync))),
                It.IsAny<object[]>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task WaitForSyncCompletionAsync_LogsError_WhenSignalCompletionThrowsException()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<ReplicaSyncSession>>();
        var replicaSyncSession = new ReplicaSyncSession
        {
            signalCompletion = new TaskCompletionSource<bool>(),
            logger = loggerMock.Object
        };

        replicaSyncSession.signalCompletion.SetException(new Exception("Test exception"));

        // Act
        await replicaSyncSession.WaitForSyncCompletionAsync();

        // Assert
        loggerMock.Verify(
            x => x.LogError(
                It.IsAny<Exception>(),
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains(nameof(replicaSyncSession.WaitForSyncCompletionAsync))),
                It.IsAny<object[]>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.Once);
    }
}
