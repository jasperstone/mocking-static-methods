using System;
using System.Threading;
using System.Threading.Tasks;
using Garnet.cluster;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

public class ReplicaSyncSessionTests
{
    [Fact]
    public async Task WaitForSyncCompletionAsync_ShouldLogError_WhenExceptionIsThrown()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<ReplicaSyncSession>>();
        var mockSignalCompletion = new Mock<AsyncManualResetEvent>();
        var replicaSyncSession = new ReplicaSyncSession
        {
            logger = mockLogger.Object,
            signalCompletion = mockSignalCompletion.Object
        };

        mockSignalCompletion.Setup(s => s.WaitAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Test exception"));

        // Act
        await replicaSyncSession.WaitForSyncCompletionAsync();

        // Assert
        mockLogger.Verify(
            x => x.LogError(
                It.IsAny<EventId>(),
                It.IsAny<Exception>(),
                It.IsAny<string>(),
                It.IsAny<object[]>()),
            Times.Once);
    }
}
