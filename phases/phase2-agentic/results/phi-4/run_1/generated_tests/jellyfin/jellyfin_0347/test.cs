using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace Jellyfin.Tests;

public class BackupServiceTests
{
    [Fact]
    public async Task TestDatabaseBackup()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<BackupService>>();
        var dbContextMock = new Mock<IDbContext>();
        var historyRepositoryMock = new Mock<IHistoryRepository>();
        var jellyfinDatabaseProviderMock = new Mock<IJellyfinDatabaseProvider>();
        var backupService = new BackupService(loggerMock.Object, dbContextMock.Object, null, null, jellyfinDatabaseProviderMock.Object, null);

        // Act
        await backupService.RestoreBackupAsync("path/to/backup.zip");

        // Assert
        loggerMock.Verify(
            logger => logger.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Database Purged")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.Once);
    }
}
