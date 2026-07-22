using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using Garnet.cluster;
using Tsavorite.core;

public class MigrateSessionSlotsTests
{
    [Fact]
    public async Task CreateAndRunMigrateTasksAsync_LogsErrorOnException()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<MigrateSession>>();
        var migrateSession = new MigrateSession();
        var exception = new Exception("Test exception");

        // Act
        await migrateSession.CreateAndRunMigrateTasksAsync(StoreType.Main, 0, 100, 16);

        // Assert
        mockLogger.Verify(
            x => x.LogError(
                It.IsAny<Exception>(),
                It.IsAny<string>(),
                It.IsAny<object>(),
                It.IsAny<object>(),
                It.IsAny<object>(),
                It.IsAny<object>()),
            Times.Once);
    }
}
