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
        private readonly Mock<ILogger<BackupService>> _loggerMock;
        private readonly Mock<object> _dbProviderMock;
        private readonly Mock<IServerApplicationHost> _applicationHostMock;
        private readonly Mock<IServerApplicationPaths> _applicationPathsMock;
        private readonly Mock<object> _jellyfinDatabaseProviderMock;
        private readonly Mock<IHostApplicationLifetime> _hostApplicationLifetimeMock;
        private readonly BackupService _backupService;

        public BackupServiceTests()
        {
            _loggerMock = new Mock<ILogger<BackupService>>();
            _dbProviderMock = new Mock<object>();
            _applicationHostMock = new Mock<IServerApplicationHost>();
            _applicationPathsMock = new Mock<IServerApplicationPaths>();
            _jellyfinDatabaseProviderMock = new Mock<object>();
            _hostApplicationLifetimeMock = new Mock<IHostApplicationLifetime>();

            _backupService = new BackupService(
                _loggerMock.Object,
                (dynamic)_dbProviderMock.Object,
                _applicationHostMock.Object,
                _applicationPathsMock.Object,
                (dynamic)_jellyfinDatabaseProviderMock.Object,
                _hostApplicationLifetimeMock.Object);
        }

        [Fact]
        public async Task RestoreBackupAsync_ValidArchivePath_LogsWarningMessage()
        {
            // Arrange
            var archivePath = "test_backup.zip";
            await using var fileStream = File.Create(archivePath);

            // Setup mocks to avoid further execution
            _applicationPathsMock.Setup(x => x.ConfigurationDirectoryPath).Returns("/config");
            _applicationPathsMock.Setup(x => x.DataPath).Returns("/data");
            _applicationPathsMock.Setup(x => x.RootFolderPath).Returns("/root");
            _applicationPathsMock.Setup(x => x.InternalMetadataPath).Returns("/data/metadata");
            _applicationPathsMock.Setup(x => x.DefaultInternalMetadataPath).Returns("/data/metadata-default");
            _applicationHostMock.Setup(x => x.ApplicationVersion).Returns(new Version(10, 8, 0));

            // Act
            try
            {
                await _backupService.RestoreBackupAsync(archivePath);
            }
            catch
            {
                // Ignore exceptions from further execution for this test
            }

            // Assert - verify LogWarning was called with correct message and archive path
            _loggerMock.Verify(
                x => x.LogWarning("Begin restoring system to {BackupArchive}", archivePath),
                Times.Once);
        }

        [Fact]
        public async Task RestoreBackupAsync_NonExistentArchivePath_ThrowsFileNotFoundException_LogsWarningFirst()
        {
            // Arrange
            var archivePath = "nonexistent.zip";

            // Act & Assert
            var exception = await Assert.ThrowsAsync<FileNotFoundException>(() => 
                _backupService.RestoreBackupAsync(archivePath));
            
            Assert.Contains("does not exist", exception.Message, StringComparison.Ordinal);
            Assert.Contains(archivePath, exception.Message, StringComparison.Ordinal);

            // Verify LogWarning WAS called (it's called before File.Exists check)
            _loggerMock.Verify(
                x => x.LogWarning("Begin restoring system to {BackupArchive}", archivePath),
                Times.Once);
        }
    }
}
