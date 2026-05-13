using Xunit;
using Moq;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Jellyfin.Server.Implementations.FullSystemBackup.Tests
{
    public class BackupServiceTests
    {
        [Fact]
        public async Task BackupService_LogInformation_CalledWithCorrectMessage()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<BackupService>>();
            var backupService = new BackupService(loggerMock.Object, null, null, null, null, null);

            // Act
            await backupService.RestoreBackupAsync("path/to/backup.zip");

            // Assert
            loggerMock.Verify(l => l.LogInformation("Begin restoring system to {BackupArchive}", "path/to/backup.zip"), Times.Once);
        }

        [Fact]
        public async Task BackupService_LogInformation_CalledWithCorrectMessageForFolderBackup()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<BackupService>>();
            var backupService = new BackupService(loggerMock.Object, null, null, null, null, null);

            // Act
            await backupService.RestoreBackupAsync("path/to/backup.zip");

            // Assert
            loggerMock.Verify(l => l.LogInformation("Backup of folder {Table}", "Config"), Times.Once);
        }
    }
}
