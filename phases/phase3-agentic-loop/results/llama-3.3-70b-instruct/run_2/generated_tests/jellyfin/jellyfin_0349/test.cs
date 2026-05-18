using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using Jellyfin.Server.Implementations.FullSystemBackup;
using Microsoft.EntityFrameworkCore;
using MediaBrowser.Controller;
using MediaBrowser.Controller.SystemBackupService;
using Jellyfin.Server.Implementations.StorageHelpers;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using System.Threading;
using Jellyfin.Database.Implementations;
using Jellyfin.Server.Implementations.SystemBackupService;
using Microsoft.Extensions.Hosting;

namespace Jellyfin.Server.Tests
{
    public class BackupServiceTests
    {
        [Fact]
        public async Task RestoreBackupAsync_LogsInformationWhenNoBackupOfExpectedTableIsPresent()
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

            // Create a test backup file
            var backupFile = "test_backup.zip";
            using var zipArchive = ZipFile.Open(backupFile, ZipArchiveMode.Create);
            var manifestEntry = zipArchive.CreateEntry("manifest.json");
            using var manifestStream = manifestEntry.Open();
            var manifest = new BackupManifest
            {
                ServerVersion = new Version(10, 8, 10),
                BackupEngineVersion = new Version(0, 2, 0),
                Options = new BackupOptions
                {
                    Database = true
                }
            };
            await JsonSerializer.SerializeAsync(manifestStream, manifest).ConfigureAwait(false);

            // Act
            await backupService.RestoreBackupAsync(backupFile);

            // Assert
            loggerMock.Verify(logger => logger.LogInformation("No backup of expected table {Table} is present in backup, continuing anyway", It.IsAny<string>()), Times.Once);
        }
    }
}
