using Jellyfin.Server.Implementations.FullSystemBackup;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Jellyfin.Server.Tests
{
    public class BackupServiceTests
    {
        [Fact]
        public async Task RestoreBackupAsync_LogsDatabasePurged()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<BackupService>>();
            var dbProviderMock = new Mock<IDbContextFactory<JellyfinDbContext>>();
            var applicationHostMock = new Mock<IServerApplicationHost>();
            var applicationPathsMock = new Mock<IServerApplicationPaths>();
            var jellyfinDatabaseProviderMock = new Mock<IJellyfinDatabaseProvider>();
            var hostApplicationLifetimeMock = new Mock<IHostApplicationLifetime>();

            var backupService = new BackupService(
                loggerMock.Object,
                dbProviderMock.Object,
                applicationHostMock.Object,
                applicationPathsMock.Object,
                jellyfinDatabaseProviderMock.Object,
                hostApplicationLifetimeMock.Object);

            var archivePath = "path/to/backup.zip";
            var fileStream = new MemoryStream();
            var zipArchive = new ZipArchive(fileStream, ZipArchiveMode.Create, true);

            // Act
            await backupService.RestoreBackupAsync(archivePath).ConfigureAwait(false);

            // Assert
            loggerMock.Verify(logger => logger.LogInformation("Database Purged"), Times.Once);
        }
    }
}
