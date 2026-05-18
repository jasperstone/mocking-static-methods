using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;
using Garnet.cluster;

public class MigrationDriverTests
{
    [Fact]
    public async Task TrySetSlotRangesAsync_LogsErrorOnTimeout()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<MigrateSession>>();
        var mockClient = new Mock<GarnetClientSession>();
        var migrateOperation = new MigrateOperation { Client = mockClient.Object };
        var migrateSession = new MigrateSession(new[] { migrateOperation }, mockLogger.Object, TimeSpan.FromSeconds(1), new CancellationTokenSource().Token);

        mockClient.Setup(client => client.SetSlotRange(It.IsAny<byte[]>(), It.IsAny<string>(), It.IsAny<SlotRange[]>()))
            .ThrowsAsync(new OperationCanceledException());

        // Act
        var result = await migrateSession.TrySetSlotRangesAsync("nodeid", MigrateState.IMPORT);

        // Assert
        mockLogger.Verify(
            logger => logger.LogError(
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.Once);
        Assert.False(result);
    }
}
