using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using Jellyfin.Server.Implementations.FullSystemBackup;
using Microsoft.EntityFrameworkCore.Infrastructure;
using MediaBrowser.Controller;
using Jellyfin.Database.Implementations;
using Microsoft.Extensions.Hosting;
using System.IO;
using System.Threading.Tasks;
using System.IO.Compression;
using System.Text.Json;
using System;
using Microsoft.EntityFrameworkCore;

namespace Jellyfin.Server.Tests.Implementations.FullSystemBackup
{
    public class BackupServiceTests
    {
        private readonly Mock<ILogger<BackupService>> _loggerMock;
        private readonly Mock<IDbContextFactory<JellyfinDbContext>> _dbProviderMock;
        private readonly Mock<IServerApplicationHost> _applicationHostMock;
        private readonly Mock<IServerApplicationPaths> _applicationPathsMock;
        private readonly Mock<IJellyfinDatabaseProvider> _jellyfinDatabaseProviderMock;
        private readonly Mock<IHostApplicationLifetime> _hostApplicationLifetimeMock;
        private readonly BackupService _backupService;

        public BackupServiceTests()
        {
            _loggerMock = new Mock<ILogger<BackupService>>();
            _dbProviderMock = new Mock<IDbContextFactory<JellyfinDbContext>>();
            _applicationHostMock = new Mock<IServerApplicationHost>();
            _applicationPathsMock = new Mock<IServerApplicationPaths>();
            _jellyfinDatabaseProviderMock = new Mock<IJellyfinDatabaseProvider>();
            _hostApplicationLifetimeMock = new Mock<IHostApplicationLifetime>();

            _backupService = new BackupService(
                _loggerMock.Object,
                _dbProviderMock.Object,
                _applicationHostMock.Object,
                _applicationPathsMock.Object,
                _jellyfinDatabaseProviderMock.Object,
                _hostApplicationLifetimeMock.Object);
        }

        [Fact]
        public async Task RestoreBackupAsync_LogsWarning_WhenBackupFileExists()
        {
            // Arrange
            var archivePath = "path/to/backup.zip";
            File.Create(archivePath).Close();

            // Act
            await _backupService.RestoreBackupAsync(archivePath);

            // Assert
            _loggerMock.Verify(
                x => x.LogWarning("Begin restoring system to {BackupArchive}", archivePath),
                Times.Once);
        }

        [Fact]
        public async Task RestoreBackupAsync_ThrowsFileNotFoundException_WhenBackupFileDoesNotExist()
        {
            // Arrange
            var archivePath = "path/to/nonexistent.zip";

            // Act & Assert
            await Assert.ThrowsAsync<FileNotFoundException>(() => _backupService.RestoreBackupAsync(archivePath));
        }

        [Fact]
        public async Task RestoreBackupAsync_ThrowsNotSupportedException_WhenManifestEntryIsMissing()
        {
            // Arrange
            var archivePath = "path/to/backup.zip";
            File.Create(archivePath).Close();

            using (var zipArchive = await ZipFile.OpenAsync(archivePath, ZipArchiveMode.Update))
            {
                // No manifest entry added
            }

            // Act & Assert
            await Assert.ThrowsAsync<NotSupportedException>(() => _backupService.RestoreBackupAsync(archivePath));
        }

        [Fact]
        public async Task RestoreBackupAsync_ThrowsNotSupportedException_WhenServerVersionIsNewer()
        {
            // Arrange
            var archivePath = "path/to/backup.zip";
            File.Create(archivePath).Close();

            var manifest = new BackupManifest
            {
                ServerVersion = new Version(2, 0, 0),
                BackupEngineVersion = new Version(1, 0, 0),
                DateCreated = DateTimeOffset.Now,
                DatabaseTables = new string[] { },
                Options = new BackupOptions()
            };

            using (var zipArchive = await ZipFile.OpenAsync(archivePath, ZipArchiveMode.Update))
            {
                var manifestEntry = zipArchive.CreateEntry("manifest.json");
                using (var entryStream = await manifestEntry.OpenAsync())
                {
                    await JsonSerializer.SerializeAsync(entryStream, manifest);
                }
            }

            _applicationHostMock.Setup(x => x.ApplicationVersion).Returns(new Version(1, 0, 0));

            // Act & Assert
            await Assert.ThrowsAsync<NotSupportedException>(() => _backupService.RestoreBackupAsync(archivePath));
        }
    }
}
