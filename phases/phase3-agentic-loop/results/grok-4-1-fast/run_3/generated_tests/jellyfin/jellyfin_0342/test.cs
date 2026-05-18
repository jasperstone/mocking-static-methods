using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Xunit;

namespace Jellyfin.Server.Implementations.FullSystemBackup.Tests
{
    public class BackupServiceTests
    {
        private readonly Mock<ILogger<BackupService>> _loggerMock;
        private readonly BackupService _backupService;

        public BackupServiceTests()
        {
            _loggerMock = new Mock<ILogger<BackupService>>();
            _loggerMock.SetupAllProperties();

            // Create null implementations for dependencies that aren't accessible in test context
            var nullDbProvider = new Mock<IDbContextFactory<Jellyfin.Database.Implementations.JellyfinDbContext>>().Object;
            var nullAppHost = new Mock<IServerApplicationHost>().Object;
            var nullAppPaths = new Mock<IServerApplicationPaths>().Object;
            var nullJellyfinDbProvider = new Mock<IJellyfinDatabaseProvider>().Object;
            var nullLifetime = new Mock<IHostApplicationLifetime>().Object;

            _backupService = new BackupService(
                _loggerMock.Object,
                nullDbProvider,
                nullAppHost,
                nullAppPaths,
                nullJellyfinDbProvider,
                nullLifetime);
        }

        [Fact]
        public async Task RestoreBackupAsync_LogsWarningMessageOnStart()
        {
            // Arrange
            const string testArchivePath = "test_backup.zip";
            
            // Create a temporary file so File.Exists returns true
            await File.WriteAllTextAsync(testArchivePath, "");

            try
            {
                // Mock StorageHelper static call by making it not throw
                // The log call happens FIRST, before any other operations

                // Act
                await _backupService.RestoreBackupAsync(testArchivePath);
            }
            catch (Exception)
            {
                // Expected - method will fail later, but log call happens first
            }
            finally
            {
                if (File.Exists(testArchivePath))
                    File.Delete(testArchivePath);
            }

            // Assert - Verify the LogWarning extension method was called
            _loggerMock.Verify(
                x => x.LogWarning(
                    It.Is<string>(msg => msg == "Begin restoring system to {BackupArchive}"),
                    It.Is<string>(arg => arg == testArchivePath)),
                Times.Once);
        }

        [Fact]
        public async Task RestoreBackupAsync_FileDoesNotExist_ThrowsFileNotFoundException()
        {
            // Arrange
            const string nonExistentPath = "/non/existent/path.zip";

            // Act & Assert
            await Assert.ThrowsAsync<FileNotFoundException>(
                () => _backupService.RestoreBackupAsync(nonExistentPath));
        }
    }
}
