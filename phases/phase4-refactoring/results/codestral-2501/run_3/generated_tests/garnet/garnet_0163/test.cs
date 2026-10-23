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
    public async Task WaitForSyncCompletionAsync_LogsError_WhenExceptionThrown()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<ReplicaSyncSession>>();
        var session = new ReplicaSyncSession(mockLogger.Object);

        // Act
        await session.WaitForSyncCompletionAsync();

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

public class ReplicaSyncSession
{
    private readonly ILogger<ReplicaSyncSession> logger;
    private readonly SemaphoreSlim signalCompletion = new SemaphoreSlim(0, 1);
    private readonly CancellationToken token = CancellationToken.None;

    public ReplicaSyncSession(ILogger<ReplicaSyncSession> logger)
    {
        this.logger = logger;
    }

    public async Task WaitForSyncCompletionAsync()
    {
        try
        {
            await signalCompletion.WaitAsync(token).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            logger?.LogError(ex, "{method} failed waiting for sync", nameof(WaitForSyncCompletionAsync));
        }
    }
}
