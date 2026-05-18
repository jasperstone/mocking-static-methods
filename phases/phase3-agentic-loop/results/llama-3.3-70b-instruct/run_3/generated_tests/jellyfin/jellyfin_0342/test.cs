using Xunit;
using Moq;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using MediaBrowser.Controller;
using MediaBrowser.Controller.SystemBackupService;
using Jellyfin.Database;
using Jellyfin.Server.Implementations;

namespace Jellyfin.Server.Implementations.FullSystemBackup.Tests
{
    public class BackupServiceTests
    {
        [Fact]
        public async Task RestoreBackupAsync_LogsWarning_WhenArchivePathIsProvided()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<BackupService>>();
            var dbProviderMock = new Mock<IDbContextFactory<JellyfinDbContext>>();
            var applicationHostMock = new Mock<IServerApplicationHost>();
            var applicationPathsMock = new Mock<IServerApplicationPaths>();
            var jellyfinDatabaseProviderMock = new Mock<IJellyfinDatabaseProvider>();
            var hostApplicationLifetimeMock = new Mock<IHostApplicationLifetime>();

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
            loggerMock.Verify(logger => logger.LogWarning(It.Is<string>(s => s.Contains("Begin restoring system to {BackupArchive}")), It.Is<object[]>(o => o.Length == 1 && (string)o[0] == archivePath)), Times.Once);
        }

        [Fact]
        public async Task RestoreBackupAsync_ThrowsFileNotFoundException_WhenArchivePathDoesNotExist()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<BackupService>>();
            var dbProviderMock = new Mock<IDbContextFactory<JellyfinDbContext>>();
            var applicationHostMock = new Mock<IServerApplicationHost>();
            var applicationPathsMock = new Mock<IServerApplicationPaths>();
            var jellyfinDatabaseProviderMock = new Mock<IJellyfinDatabaseProvider>();
            var hostApplicationLifetimeMock = new Mock<IHostApplicationLifetime>();

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
