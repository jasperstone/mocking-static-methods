using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using Jellyfin.Server.Implementations.FullSystemBackup;
using System.IO;
using System.IO.Compression;
using Microsoft.EntityFrameworkCore;
using MediaBrowser.Controller;
using MediaBrowser.Controller.SystemBackupService;

namespace Jellyfin.Server.Tests
{
    public class BackupServiceTests
    {
        [Fact]
        public async Task LogInformation_CalledWithCorrectMessage()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<BackupService>>();
            var dbProviderMock = new Mock<IDbContextFactory<MediaBrowser.Controller.Entities.JellyfinDbContext>>();
            var applicationHostMock = new Mock<IServerApplicationHost>();
            var applicationPathsMock = new Mock<IServerApplicationPaths>();
            var jellyfinDatabaseProviderMock = new Mock<IJellyfinDatabaseProvider>();
            var hostApplicationLifetimeMock = new Mock<IHostApplicationLifetime>();
            var backupService = new BackupService(loggerMock.Object, dbProviderMock.Object, applicationHostMock.Object, applicationPathsMock.Object, jellyfinDatabaseProviderMock.Object, hostApplicationLifetimeMock.Object);

            // Create a temporary backup file
            var backupFilePath = Path.GetTempFileName();
            using var zipArchive = ZipFile.Open(backupFilePath, ZipArchiveMode.Create);
            zipArchive.CreateEntry("manifest.json");

            try
            {
                // Act
                await backupService.RestoreBackupAsync(backupFilePath);

                // Assert
                loggerMock.Verify(l => l.LogInformation("Begin restoring system to {BackupArchive}", backupFilePath), Times.Once);
            }
            finally
            {
                // Clean up the temporary backup file
                File.Delete(backupFilePath);
            }
        }

        [Fact]
        public async Task LogInformation_CalledWithCorrectMessage_ForFolderBackup()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<BackupService>>();
            var dbProviderMock = new Mock<IDbContextFactory<MediaBrowser.Controller.Entities.JellyfinDbContext>>();
            var applicationHostMock = new Mock<IServerApplicationHost>();
            var applicationPathsMock = new Mock<IServerApplicationPaths>();
            var jellyfinDatabaseProviderMock = new Mock<IJellyfinDatabaseProvider>();
            var hostApplicationLifetimeMock = new Mock<IHostApplicationLifetime>();
            var backupService = new BackupService(loggerMock.Object, dbProviderMock.Object, applicationHostMock.Object, applicationPathsMock.Object, jellyfinDatabaseProviderMock.Object, hostApplicationLifetimeMock.Object);

            // Create a temporary backup file
            var backupFilePath = Path.GetTempFileName();
            using var zipArchive = ZipFile.Open(backupFilePath, ZipArchiveMode.Create);
            zipArchive.CreateEntry("manifest.json");

            try
            {
                // Act
                await backupService.RestoreBackupAsync(backupFilePath);

                // Assert
                loggerMock.Verify(l => l.LogInformation("Backup of folder {Table}", "Config"), Times.Once);
            }
            finally
            {
                // Clean up the temporary backup file
                File.Delete(backupFilePath);
            }
        }
    }
}
