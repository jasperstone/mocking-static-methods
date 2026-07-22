using Jellyfin.Server.Implementations.FullSystemBackup;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.IO;
using System.IO.Compression;
using System.Threading.Tasks;
using Xunit;

namespace Jellyfin.Server.Tests
{
    public class BackupServiceTests
    {
        [Fact]
        public async Task RestoreBackupAsync_LogsInformation()
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
            var fileStream = File.OpenRead(archivePath);
            await using (fileStream.ConfigureAwait(false))
            {
                using var zipArchive = new ZipArchive(fileStream, ZipArchiveMode.Read, false);
                // ...

                // Act
                await backupService.RestoreBackupAsync(archivePath);

                // Assert
                loggerMock.Verify(l => l.LogInformation(It.IsAny<string>(), It.IsAny<object[]>()), Times.AtLeastOnce);
            }
        }
    }
}
