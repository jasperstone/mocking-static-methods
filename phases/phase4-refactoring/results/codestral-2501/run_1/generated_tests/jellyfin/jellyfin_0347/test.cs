using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using Jellyfin.Server.Implementations.FullSystemBackup;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using MediaBrowser.Controller;
using Jellyfin.Database.Implementations;
using System.IO;
using System.IO.Compression;
using System.Text.Json;
using System.Threading.Tasks;
using System.Threading;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using System.Linq;
using System;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Jellyfin.Server.Implementations.StorageHelpers;
using MediaBrowser.Controller.SystemBackupService;

namespace Jellyfin.Server.Implementations.FullSystemBackup.Tests
{
    public class BackupServiceTests
    {
        private readonly Mock<ILogger<BackupService>> _mockLogger;
        private readonly Mock<IDbContextFactory<JellyfinDbContext>> _mockDbProvider;
        private readonly Mock<IServerApplicationHost> _mockApplicationHost;
        private readonly Mock<IServerApplicationPaths> _mockApplicationPaths;
        private readonly Mock<IJellyfinDatabaseProvider> _mockJellyfinDatabaseProvider;
        private readonly Mock<IHostApplicationLifetime> _mockHostApplicationLifetime;
        private readonly BackupService _backupService;

        public BackupServiceTests()
        {
            _mockLogger = new Mock<ILogger<BackupService>>();
            _mockDbProvider = new Mock<IDbContextFactory<JellyfinDbContext>>();
            _mockApplicationHost = new Mock<IServerApplicationHost>();
            _mockApplicationPaths = new Mock<IServerApplicationPaths>();
            _mockJellyfinDatabaseProvider = new Mock<IJellyfinDatabaseProvider>();
            _mockHostApplicationLifetime = new Mock<IHostApplicationLifetime>();

            _backupService = new BackupService(
                _mockLogger.Object,
                _mockDbProvider.Object,
                _mockApplicationHost.Object,
                _mockApplicationPaths.Object,
                _mockJellyfinDatabaseProvider.Object,
                _mockHostApplicationLifetime.Object);
        }

        [Fact]
        public async Task RestoreBackupAsync_LogsDatabasePurged()
        {
            // Arrange
            var archivePath = "path/to/archive.zip";
            var manifestEntryName = "manifest.json";
            var manifestContent = "{\"ServerVersion\": \"1.0.0\", \"BackupEngineVersion\": \"1.0.0\", \"Options\": {\"Database\": true}}";
            var historyEntryName = "Database/HistoryRow.json";
            var historyContent = "[{\"MigrationId\": \"1\", \"ProductVersion\": \"1.0.0\"}]";

            var zipArchive = new Mock<ZipArchive>();
            var zipArchiveEntry = new Mock<ZipArchiveEntry>();
            var manifestStream = new MemoryStream();
            var historyStream = new MemoryStream();
            var dbContext = new Mock<JellyfinDbContext>(new DbContextOptions<JellyfinDbContext>(), Mock.Of<ILogger<JellyfinDbContext>>());

            _mockApplicationHost.Setup(x => x.ApplicationVersion).Returns(new Version(1, 0, 0));
            _mockDbProvider.Setup(x => x.CreateDbContextAsync(It.IsAny<CancellationToken>())).ReturnsAsync(dbContext.Object);

            zipArchive.Setup(x => x.GetEntry(manifestEntryName)).Returns(zipArchiveEntry.Object);
            zipArchiveEntry.Setup(x => x.OpenAsync()).ReturnsAsync(manifestStream);
            zipArchive.Setup(x => x.GetEntry(historyEntryName)).Returns(zipArchiveEntry.Object);
            zipArchiveEntry.Setup(x => x.OpenAsync()).ReturnsAsync(historyStream);

            using (var writer = new StreamWriter(manifestStream, leaveOpen: true))
            {
                await writer.WriteAsync(manifestContent);
                await writer.FlushAsync();
                manifestStream.Position = 0;
            }

            using (var writer = new StreamWriter(historyStream, leaveOpen: true))
            {
                await writer.WriteAsync(historyContent);
                await writer.FlushAsync();
                historyStream.Position = 0;
            }

            // Act
            await _backupService.RestoreBackupAsync(archivePath);

            // Assert
            _mockLogger.Verify(
                x => x.LogInformation("Database Purged"),
                Times.Once);
        }
    }
}
