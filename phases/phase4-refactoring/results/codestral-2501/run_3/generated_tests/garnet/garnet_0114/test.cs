using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Garnet.cluster;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

public class MigrateSessionSlotsTests
{
    [Fact]
    public async Task CreateAndRunMigrateTasksAsync_LogsError_WhenExceptionThrown()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<MigrateSession>>();
        var mockClusterProvider = new Mock<ClusterProvider>();
        var mockMigrateOperation = new Mock<MigrateOperation>();
        var mockCts = new CancellationTokenSource();

        var migrateSession = new MigrateSession(mockLogger.Object, mockClusterProvider.Object, mockMigrateOperation.Object, mockCts.Token);

        // Act
        await migrateSession.CreateAndRunMigrateTasksAsync(StoreType.Main, 0, 100, 16);

        // Assert
        mockLogger.Verify(
            logger => logger.LogError(
                It.IsAny<Exception>(),
                It.Is<string>(s => s.Contains("{CreateAndRunMigrateTasks}: {storeType} {beginAddress} {tailAddress} {pageSize}")),
                It.IsAny<object[]>()),
            Times.Once);
    }
}
