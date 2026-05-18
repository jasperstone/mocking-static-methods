using System;
using System.IO;
using System.IO.Compression;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Jellyfin.Server.Implementations.FullSystemBackup;
using MediaBrowser.Controller;
using Microsoft.Extensions.Hosting;

namespace Jellyfin.Server.Tests.FullSystemBackup
{
    public class BackupServiceTests
    {
        private readonly Mock<ILogger<BackupService>> _loggerMock;
        private readonly BackupService _backupService;

        public BackupServiceTests()
        {
            _loggerMock = new Mock<ILogger<BackupService>>();
            _loggerMock.Setup(l => l.Log(It.IsAny<LogLevel>(), It.IsAny<EventId>(), It.IsAny<It.IsAnyType>(), It.IsAny<Exception>(), It.IsAny<Func<It.IsAnyType, Exception?, string>>()));

            var dbProviderMock = new Mock<object>();
            var applicationHostMock = new Mock<IServerApplicationHost>();
            var applicationPathsMock = new Mock<IServerApplicationPaths>();
            var jellyfinDatabaseProviderMock = new Mock<object>();
            var applicationLifetimeMock = new Mock<IHostApplicationLifetime>();

            _backupService = new BackupService(
                _loggerMock.Object,
                (dynamic)dbProviderMock.Object,
                applicationHostMock.Object,
                applicationPathsMock.Object,
                (dynamic)jellyfinDatabaseProviderMock.Object,
                applicationLifetimeMock.Object);
        }

        [Fact]
        public async Task RestoreBackupAsync_LogsWarningWithArchivePath()
        {
            // Arrange
            var archivePath = Path.GetTempFileName();
            var tempZipPath = Path.ChangeExtension(archivePath, ".zip");
            
            try
            {
                // Create a minimal valid ZIP file that exists but fails later
                using (var zip = ZipFile.Open(tempZipPath, ZipArchiveMode.Create))
                {
                    zip.CreateEntryFromFile(archivePath, "dummy.txt");
                }

                // Act
                await Assert.ThrowsAnyAsync<Exception>(() => _backupService.RestoreBackupAsync(tempZipPath));

                // Assert - verify LogWarning was called with correct message and archive path
                _loggerMock.Verify(
                    x => x.Log(
                        LogLevel.Warning,
                        It.IsAny<EventId>(),
                        It.Is<It.IsAnyType>(state => 
                            state.ToString().Contains("Begin restoring system to") && 
                            state.ToString().Contains(tempZipPath)),
                        It.IsAny<Exception>(),
                        It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                    Times.Once);
            }
            finally
            {
                CleanupFile(archivePath);
                CleanupFile(tempZipPath);
            }
        }

        private static void CleanupFile(string path)
        {
            if (File.Exists(path))
            {
                File.Delete(path);
            }
        }
    }
}
