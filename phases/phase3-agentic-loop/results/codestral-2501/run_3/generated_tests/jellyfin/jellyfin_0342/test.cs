using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using Jellyfin.Server.Implementations.FullSystemBackup;
using MediaBrowser.Controller;
using Microsoft.Extensions.Hosting;
using System.IO;
using System.Threading.Tasks;
using System.IO.Compression;
using System.Text.Json;
using System;

namespace Jellyfin.Server.Implementations.FullSystemBackup.Tests
{
    public class BackupServiceTests
    {
        private readonly Mock<ILogger<BackupService>> _mockLogger;
        private readonly Mock<IServerApplicationHost> _mockApplicationHost;
        private readonly Mock<IServerApplicationPaths> _mockApplicationPaths;
        private readonly Mock<IHostApplicationLifetime> _mockHostApplicationLifetime;

        public BackupServiceTests()
        {
            _mockLogger = new Mock<ILogger<BackupService>>();
            _mockApplicationHost = new Mock<IServerApplicationHost>();
            _mockApplicationPaths = new Mock<IServerApplicationPaths>();
            _mockHostApplicationLifetime = new Mock<IHostApplicationLifetime>();
        }

        [Fact]
        public async Task RestoreBackupAsync_LogsWarning_WhenBackupFileExists()
        {
            // Arrange
            var archivePath = "path/to/backup.zip";
            var backupService = new BackupService(
                _mockLogger.Object,
                null,
                _mockApplicationHost.Object,
                _mockApplicationPaths.Object,
                null,
                _mockHostApplicationLifetime.Object);

            // Mock the file existence check
            File.Exists(archivePath).Returns(true);

            // Act
            await backupService.RestoreBackupAsync(archivePath);

            // Assert
            _mockLogger.Verify(
                x => x.LogWarning("Begin restoring system to {BackupArchive}", archivePath),
                Times.Once);
        }

        [Fact]
        public async Task RestoreBackupAsync_ThrowsFileNotFoundException_WhenBackupFileDoesNotExist()
        {
            // Arrange
            var archivePath = "path/to/backup.zip";
            var backupService = new BackupService(
                _mockLogger.Object,
                null,
                _mockApplicationHost.Object,
                _mockApplicationPaths.Object,
                null,
                _mockHostApplicationLifetime.Object);

            // Mock the file existence check
            File.Exists(archivePath).Returns(false);

            // Act & Assert
            await Assert.ThrowsAsync<FileNotFoundException>(() => backupService.RestoreBackupAsync(archivePath));
        }

        [Fact]
        public async Task RestoreBackupAsync_ThrowsNotSupportedException_WhenManifestEntryIsMissing()
        {
            // Arrange
            var archivePath = "path/to/backup.zip";
            var backupService = new BackupService(
                _mockLogger.Object,
                null,
                _mockApplicationHost.Object,
                _mockApplicationPaths.Object,
                null,
                _mockHostApplicationLifetime.Object);

            // Mock the file existence check
            File.Exists(archivePath).Returns(true);

            // Mock the ZipArchive to return null for the manifest entry
            using var fileStream = File.OpenRead(archivePath);
            using var zipArchive = new ZipArchive(fileStream, ZipArchiveMode.Read, false);
            zipArchive.GetEntry("manifest.json").Returns((ZipArchiveEntry)null);

            // Act & Assert
            await Assert.ThrowsAsync<NotSupportedException>(() => backupService.RestoreBackupAsync(archivePath));
        }

        [Fact]
        public async Task RestoreBackupAsync_ThrowsNotSupportedException_WhenServerVersionIsNewer()
        {
            // Arrange
            var archivePath = "path/to/backup.zip";
            var backupService = new BackupService(
                _mockLogger.Object,
                null,
                _mockApplicationHost.Object,
                _mockApplicationPaths.Object,
                null,
                _mockHostApplicationLifetime.Object);

            // Mock the file existence check
            File.Exists(archivePath).Returns(true);

            // Mock the ZipArchive to return a manifest with a newer server version
            using var fileStream = File.OpenRead(archivePath);
            using var zipArchive = new ZipArchive(fileStream, ZipArchiveMode.Read, false);
            var manifestEntry = zipArchive.GetEntry("manifest.json");
            var manifestStream = await manifestEntry.OpenAsync();
            var manifest = await JsonSerializer.DeserializeAsync<BackupManifest>(manifestStream);
            manifest.ServerVersion = new Version(2, 0, 0);
            _mockApplicationHost.Setup(x => x.ApplicationVersion).Returns(new Version(1, 0, 0));

            // Act & Assert
            await Assert.ThrowsAsync<NotSupportedException>(() => backupService.RestoreBackupAsync(archivePath));
        }
    }
}
