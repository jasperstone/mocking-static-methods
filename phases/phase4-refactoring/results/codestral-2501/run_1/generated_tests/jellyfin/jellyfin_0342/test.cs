using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using Jellyfin.Server.Implementations.FullSystemBackup;
using Microsoft.EntityFrameworkCore.Infrastructure;
using MediaBrowser.Controller;
using MediaBrowser.Controller.SystemBackupService;
using Microsoft.Extensions.Hosting;
using System.IO;
using System.Threading.Tasks;

public class BackupServiceTests
{
    [Fact]
    public void LogWarning_WhenRestoringBackupAsync_ShouldLogWarning()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<BackupService>>();
        var dbProviderMock = new Mock<IDbContextFactory<JellyfinDbContext>>();
        var applicationHostMock = new Mock<IServerApplicationHost>();
        var applicationPathsMock = new Mock<IServerApplicationPaths>();
        var jellyfinDatabaseProviderMock = new Mock<IJellyfinDatabaseProvider>();
        var applicationLifetimeMock = new Mock<IHostApplicationLifetime>();

        var backupService = new BackupService(
            loggerMock.Object,
            dbProviderMock.Object,
            applicationHostMock.Object,
            applicationPathsMock.Object,
            jellyfinDatabaseProviderMock.Object,
            applicationLifetimeMock.Object);

        var archivePath = "path/to/archive.zip";

        // Act
        var task = backupService.RestoreBackupAsync(archivePath);

        // Assert
        loggerMock.Verify(
            x => x.LogWarning("Begin restoring system to {BackupArchive}", It.IsAny<object[]>()),
            Times.Once);
    }
}
