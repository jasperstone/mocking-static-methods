using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
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
        var mockClusterSession = new Mock<ClusterSession>();
        var mockClusterProvider = new Mock<ClusterProvider>();
        var migrateSession = new MigrateSession(
            mockClusterSession.Object,
            mockClusterProvider.Object,
            "targetAddress",
            1234,
            "targetNodeId",
            "username",
            "password",
            "sourceNodeId",
            false,
            false,
            100,
            new HashSet<int> { 1, 2, 3 },
            null,
            TransferOption.SLOTS
        );

        migrateSession._cts.Cancel(); // Simulate a cancellation

        // Act
        var result = await migrateSession.TrySetSlotRangesAsync("nodeid", MigrateState.IMPORT);

        // Assert
        mockLogger.Verify(
            logger => logger.LogError(
                It.IsAny<EventId>(),
                It.IsAny<Exception>(),
                It.IsAny<string>(),
                It.IsAny<object[]>()),
            Times.Once);

        Assert.False(result);
        Assert.Equal(MigrateState.FAIL, migrateSession.Status);
    }
}
