using Xunit;
using Moq;
using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Jellyfin.Server.Implementations.FullSystemBackup;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Jellyfin.Server.Tests.Implementations.FullSystemBackupTests.BackupServiceTests.LogInformationTests.CallOnLine373Tests
{
    public class LogInformationTests
    {
        [Fact]
        public async Task CallOnLine373_LogsInformation()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<BackupService>>();
            var backupService = new BackupService(
                loggerMock.Object,
                null,
                null,
                null,
                null,
                null);

            // Act
            await backupService.RestoreBackupAsync("path/to/backup.zip");

            // Assert
            loggerMock.Verify(logger => logger.LogInformation("Backup of folder {Table}", "path/to/backup.zip"), Times.Once);
        }
    }
}
