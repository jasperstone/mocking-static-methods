using System;
using System.IO;
using System.Threading.Tasks;
using Jellyfin.Server.Implementations.FullSystemBackup;
using MediaBrowser.Controller;
using MediaBrowser.Controller.SystemBackupService;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Jellyfin.Server.Implementations.Tests.FullSystemBackup
{
    public class BackupServiceTests
    {
        private readonly Mock<ILogger<BackupService>> _mockLogger;
        private readonly Mock<object> _mockDbProvider;
        private readonly Mock<IServerApplicationHost> _mockApplicationHost;
        private readonly Mock<IServerApplicationPaths> _mockApplicationPaths;
        private readonly Mock<IJellyfinDatabaseProvider> _mockJellyfinDatabaseProvider;
        private readonly Mock<IHostApplicationLifetime> _mockApplicationLifetime;
        private readonly BackupService _backupService;

        public BackupServiceTests()
        {
            _mockLogger = new Mock<ILogger<BackupService>>();
            _mockDbProvider = new Mock<object>();
            _mockApplicationHost = new Mock<IServerApplicationHost>();
            _mockApplicationPaths = new Mock<IServerApplicationPaths>();
            _mockJellyfinDatabaseProvider = new Mock<IJellyfinDatabaseProvider>();
            _mockApplicationLifetime = new Mock<IHostApplicationLifetime>();

            // Setup mocks to avoid calls that would fail
            _mockApplicationPaths.Setup(x => x.ConfigurationDirectoryPath).Returns("/config");
            _mockApplicationPaths.Setup(x => x.DataPath).Returns("/data");
            _mockApplicationPaths.Setup(x => x.RootFolderPath).Returns("/root");
            _mockApplicationPaths.Setup(x => x.InternalMetadataPath).Returns("/data/metadata");
            _mockApplicationPaths.Setup(x => x.DefaultInternalMetadataPath).Returns("/data/metadata-default");

            _backupService = new BackupService(
                _mockLogger.Object,
                (dynamic)_mockDbProvider.Object,
                _mockApplicationHost.Object,
                _mockApplicationPaths.Object,
                _mockJellyfinDatabaseProvider.Object,
                _mockApplicationLifetime.Object);
        }

        [Fact]
        public async Task RestoreBackupAsync_ValidArchivePath_LogsWarningMessage()
        {
            // Arrange
            var archivePath = "test-backup.zip";
            using var fileStream = File.Create(archivePath);
            fileStream.DisposeAsync().AsTask().Wait();

            // Act
            await _backupService.RestoreBackupAsync(archivePath);

            // Assert
            _mockLogger.Verify(
                x => x.LogWarning("Begin restoring system to {BackupArchive}", archivePath),
                Times.Once);
        }

        [Fact]
        public async Task RestoreBackupAsync_NonExistentArchivePath_ThrowsFileNotFoundException()
        {
            // Arrange
            var nonExistentPath = "nonexistent.zip";

            // Act & Assert
            var exception = await Assert.ThrowsAsync<FileNotFoundException>(() => 
                _backupService.RestoreBackupAsync(nonExistentPath));
            Assert.Contains("does not exist", exception.Message, StringComparison.OrdinalIgnoreCase);
        }
    }
}
