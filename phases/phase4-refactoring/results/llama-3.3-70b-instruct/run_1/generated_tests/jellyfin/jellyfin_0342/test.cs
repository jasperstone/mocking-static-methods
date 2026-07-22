using Xunit;
using Moq;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Jellyfin.Database;
using Jellyfin.Server.Implementations;
using MediaBrowser.Controller;
using MediaBrowser.Controller.SystemBackupService;

namespace Jellyfin.Server.Implementations.FullSystemBackup.Tests
{
    public class BackupServiceTests
    {
        [Fact]
        public async Task RestoreBackupAsync_LogsWarning_WhenArchivePathIsProvided()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<BackupService>>();
            var dbProviderMock = new Mock<Jellyfin.Server.Implementations.IDbContextFactory<Jellyfin.Database.JellyfinDbContext>>();
            var applicationHostMock = new Mock<MediaBrowser.Controller.IServerApplicationHost>();
            var applicationPathsMock = new Mock<MediaBrowser.Controller.IServerApplicationPaths>();
            var jellyfinDatabaseProviderMock = new Mock<Jellyfin.Server.Implementations.IJellyfinDatabaseProvider>();
            var hostApplicationLifetimeMock = new Mock<Microsoft.Extensions.Hosting.IHostApplicationLifetime>();

            var backupService = new BackupService(
                loggerMock.Object,
                dbProviderMock.Object,
                applicationHostMock.Object,
                applicationPathsMock.Object,
                jellyfinDatabaseProviderMock.Object,
                hostApplicationLifetimeMock.Object);

            var archivePath = "path/to/archive.zip";

            // Act
            await backupService.RestoreBackupAsync(archivePath);

            // Assert
            loggerMock.Verify(logger => logger.LogWarning("Begin restoring system to {BackupArchive}", archivePath), Times.Once);
        }

        [Fact]
        public async Task RestoreBackupAsync_ThrowsFileNotFoundException_WhenArchivePathDoesNotExist()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<BackupService>>();
            var dbProviderMock = new Mock<Jellyfin.Server.Implementations.IDbContextFactory<Jellyfin.Database.JellyfinDbContext>>();
            var applicationHostMock = new Mock<MediaBrowser.Controller.IServerApplicationHost>();
            var applicationPathsMock = new Mock<MediaBrowser.Controller.IServerApplicationPaths>();
            var jellyfinDatabaseProviderMock = new Mock<Jellyfin.Server.Implementations.IJellyfinDatabaseProvider>();
            var hostApplicationLifetimeMock = new Mock<Microsoft.Extensions.Hosting.IHostApplicationLifetime>();

            var backupService = new BackupService(
                loggerMock.Object,
                dbProviderMock.Object,
                applicationHostMock.Object,
                applicationPathsMock.Object,
                jellyfinDatabaseProviderMock.Object,
                hostApplicationLifetimeMock.Object);

            var archivePath = "path/to/non/existent/archive.zip";

            // Act and Assert
            await Assert.ThrowsAsync<FileNotFoundException>(() => backupService.RestoreBackupAsync(archivePath));
        }
    }
}
