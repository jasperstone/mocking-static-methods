using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using Jellyfin.Server.Implementations.FullSystemBackup;
using Microsoft.EntityFrameworkCore;
using MediaBrowser.Controller;
using MediaBrowser.Controller.SystemBackupService;
using Jellyfin.Database.Implementations;
using Jellyfin.Server.Implementations.StorageHelpers;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using System.Threading;
using System.Collections.Generic;
using Microsoft.Extensions.Hosting;

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

            // Create a test backup file
            var backupFilePath = "test_backup.zip";
            using var backupFile = File.Create(backupFilePath);
            using var zipArchive = new ZipArchive(backupFile, ZipArchiveMode.Create, true);
            var manifestEntry = zipArchive.CreateEntry("manifest.json");
            using var manifestStream = manifestEntry.Open();
            var manifest = new BackupManifest
            {
                ServerVersion = new Version(1, 0, 0),
                BackupEngineVersion = new Version(0, 2, 0),
                Options = new BackupOptions
                {
                    Database = true
                }
            };
            await JsonSerializer.SerializeAsync(manifestStream, manifest).ConfigureAwait(false);

            // Act
            await backupService.RestoreBackupAsync(backupFilePath);

            // Assert
            loggerMock.Verify(logger => logger.LogInformation("Begin purging database"), Times.Once);
            loggerMock.Verify(logger => logger.LogInformation("Database Purged"), Times.Once);
        }
    }
}
