using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using Jellyfin.Server.Implementations.FullSystemBackup;
using MediaBrowser.Controller;
using MediaBrowser.Controller.SystemBackupService;
using Microsoft.Extensions.Hosting;
using System.IO;
using System.Threading.Tasks;
using System.IO.Compression;
using System.Text.Json;
using System;

namespace Jellyfin.Server.Tests.Implementations.FullSystemBackup
{
    public class BackupServiceTests
    {
        private readonly Mock<ILogger<BackupService>> _loggerMock;
        private readonly Mock<IServerApplicationHost> _applicationHostMock;
        private readonly Mock<IServerApplicationPaths> _applicationPathsMock;
        private readonly Mock<IHostApplicationLifetime> _hostApplicationLifetimeMock;
        private readonly BackupService _backupService;

        public BackupServiceTests()
        {
            _loggerMock = new Mock<ILogger<BackupService>>();
            _applicationHostMock = new Mock<IServerApplicationHost>();
            _applicationPathsMock = new Mock<IServerApplicationPaths>();
            _hostApplicationLifetimeMock = new Mock<IHostApplicationLifetime>();

            _backupService = new BackupService(
                _loggerMock.Object,
                null,
                _applicationHostMock.Object,
                _applicationPathsMock.Object,
                null,
                _hostApplicationLifetimeMock.Object);
        }

        [Fact]
        public void RestoreBackupAsync_LogsWarning()
        {
            // Arrange
            var archivePath = "path/to/archive.zip";
            File.Create(archivePath).Close();

            // Act
            _backupService.RestoreBackupAsync(archivePath).Wait();

            // Assert
            _loggerMock.Verify(
                x => x.LogWarning("Begin restoring system to {BackupArchive}", It.IsAny<object[]>()),
                Times.Once);
        }

        [Fact]
        public async Task RestoreBackupAsync_ThrowsFileNotFoundException_WhenFileDoesNotExist()
        {
            // Arrange
            var archivePath = "path/to/nonexistent.zip";

            // Act & Assert
            await Assert.ThrowsAsync<FileNotFoundException>(() => _backupService.RestoreBackupAsync(archivePath));
        }

        [Fact]
        public async Task RestoreBackupAsync_ThrowsNotSupportedException_WhenManifestIsMissing()
        {
            // Arrange
            var archivePath = "path/to/archive.zip";
            File.Create(archivePath).Close();

            // Act & Assert
            await Assert.ThrowsAsync<NotSupportedException>(() => _backupService.RestoreBackupAsync(archivePath));
        }

        [Fact]
        public async Task RestoreBackupAsync_ThrowsNotSupportedException_WhenServerVersionIsNewer()
        {
            // Arrange
            var archivePath = "path/to/archive.zip";
            File.Create(archivePath).Close();
            var manifest = new BackupManifest
            {
                ServerVersion = new Version(2, 0, 0),
                BackupEngineVersion = new Version(1, 0, 0),
                DateCreated = DateTimeOffset.Now,
                DatabaseTables = new string[] { },
                Options = new BackupOptions()
            };
            var json = JsonSerializer.Serialize(manifest);
            using (var archive = ZipFile.Open(archivePath, ZipArchiveMode.Update))
            {
                var entry = archive.CreateEntry("manifest.json");
                using (var writer = new StreamWriter(entry.Open()))
                {
                    writer.Write(json);
                }
            }
            _applicationHostMock.Setup(x => x.ApplicationVersion).Returns(new Version(1, 0, 0));

            // Act & Assert
            await Assert.ThrowsAsync<NotSupportedException>(() => _backupService.RestoreBackupAsync(archivePath));
        }
    }
}
