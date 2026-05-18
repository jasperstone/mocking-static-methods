using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using Garnet.cluster;
using Garnet.common;
using Tsavorite.core;

public class ReplicaSyncSessionTests
{
    [Fact]
    public async Task WaitForSyncCompletionAsync_LogsError_WhenExceptionThrown()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<ReplicaSyncSession>>();
        var replicaSyncSession = new ReplicaSyncSession
        {
            logger = mockLogger.Object,
            signalCompletion = new AsyncManualResetEvent(),
            ssInfo = new SyncStatusInfo()
        };

        // Act
        await replicaSyncSession.WaitForSyncCompletionAsync();

        // Assert
        mockLogger.Verify(
            x => x.LogError(
                It.IsAny<Exception>(),
                It.IsAny<string>(),
                It.IsAny<object[]>()),
            Times.Once);
    }
}
