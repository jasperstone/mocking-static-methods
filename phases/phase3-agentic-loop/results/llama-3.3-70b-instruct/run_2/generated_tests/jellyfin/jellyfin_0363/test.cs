using Xunit;
using Moq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Jellyfin.Server.Implementations.FullSystemBackup;
using System.IO;
using System.IO.Compression;
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
            var dbProviderMock = new Mock<MediaBrowser.Controller.SystemBackupService.IDbContextFactory<MediaBrowser.Controller.SystemBackupService.JellyfinDbContext>>();
            var applicationHostMock = new Mock<IServerApplicationHost>();
            var applicationPathsMock = new Mock<IServerApplicationPaths>();
            var jellyfinDatabaseProviderMock = new Mock<IJellyfinDatabaseProvider>();
            var hostApplicationLifetimeMock = new Mock<IHostApplicationLifetime>();
            var backupService = new BackupService(loggerMock.Object, dbProviderMock.Object, applicationHostMock.Object, applicationPathsMock.Object, jellyfinDatabaseProviderMock.Object, hostApplicationLifetimeMock.Object);

            var tempFile = Path.GetTempFileName();
            try
            {
                using var zipArchive = ZipFile.Open(tempFile, ZipArchiveMode.Create);
                var manifestEntry = zipArchive.CreateEntry("manifest.json");
                using var manifestStream = manifestEntry.Open();
                var manifest = new BackupManifest
                {
                    ServerVersion = new Version(10, 8, 4),
                    BackupEngineVersion = new Version(0, 2, 0),
                    Options = new BackupOptions
                    {
                        Database = true
                    }
                };
                await System.Text.Json.JsonSerializer.SerializeAsync(manifestStream, manifest);

                // Act
                await backupService.RestoreBackupAsync(tempFile);

                // Assert
                loggerMock.Verify(l => l.LogInformation("Begin restoring system to {BackupArchive}", tempFile), Times.Once);
            }
            finally
            {
                File.Delete(tempFile);
            }
        }

        [Fact]
        public async Task LogInformation_CalledWithCorrectMessage_ForFolderBackup()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<BackupService>>();
            var dbProviderMock = new Mock<MediaBrowser.Controller.SystemBackupService.IDbContextFactory<MediaBrowser.Controller.SystemBackupService.JellyfinDbContext>>();
            var applicationHostMock = new Mock<IServerApplicationHost>();
            var applicationPathsMock = new Mock<IServerApplicationPaths>();
            var jellyfinDatabaseProviderMock = new Mock<IJellyfinDatabaseProvider>();
            var hostApplicationLifetimeMock = new Mock<IHostApplicationLifetime>();
            var backupService = new BackupService(loggerMock.Object, dbProviderMock.Object, applicationHostMock.Object, applicationPathsMock.Object, jellyfinDatabaseProviderMock.Object, hostApplicationLifetimeMock.Object);

            var tempFile = Path.GetTempFileName();
            try
            {
                using var zipArchive = ZipFile.Open(tempFile, ZipArchiveMode.Create);
                var manifestEntry = zipArchive.CreateEntry("manifest.json");
                using var manifestStream = manifestEntry.Open();
                var manifest = new BackupManifest
                {
                    ServerVersion = new Version(10, 8, 4),
                    BackupEngineVersion = new Version(0, 2, 0),
                    Options = new BackupOptions
                    {
                        Database = true
                    }
                };
                await System.Text.Json.JsonSerializer.SerializeAsync(manifestStream, manifest);

                // Act
                await backupService.RestoreBackupAsync(tempFile);

                // Assert
                loggerMock.Verify(l => l.LogInformation("Backup of folder {Table}", "Config"), Times.Once);
            }
            finally
            {
                File.Delete(tempFile);
            }
        }
    }
}
